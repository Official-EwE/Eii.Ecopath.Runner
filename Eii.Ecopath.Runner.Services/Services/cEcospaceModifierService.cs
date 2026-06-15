using Eii.Ecopath.Runner.Datamodel.Automation;
using EwEBridge.Ecospace;
using EwECore;
using EwECore.Common;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Service responsible for executing an Ecospace run using a configured
    /// <see cref="cEcospaceModifier"/>.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cEcospaceModifierService : cRuntimeModifierService
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcospaceModifierService(cCoreService coreService, cNodeService nodeService, ILogger<cEcospaceModifierService> logger)
            : base(coreService, nodeService, logger)
        {
        }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        protected override int DateToTimeStep(DateTime date)
        {
            return Core.AbsoluteTimeToEcospaceTimestep(date);
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Execute the Ecospace run described by <paramref name="mod"/>.
        /// </summary>
        /// <param name="mod">A fully configured <see cref="cEcospaceModifier"/>.</param>
        /// <returns>True if the run completed without errors.</returns>
        // --------------------------------------------------------------------
        internal bool Run(cEcospaceModifier mod)
        {
            bool runSuccess = true;

            CompleteAndPrepareChanges(mod);

            cEcospaceDataStructures ds = Core.EcospaceDataStructures;

            // Disable unnecessary data caching - Ecospace won't be saved after
            ds.PreserveLayerData = false;
            // Reroute spatial log folder
            Core.SpatialOperationLog.LogFilePath = Core.OutputPath;

            if (mod.MyRunModel.RunSpinupYears > 0)
            {
                ds.UseSpinUp = true;
                ds.SpinUpYears = mod.MyRunModel.RunSpinupYears;
                Console.WriteLine("Ecospace spin-up = {0} years", ds.SpinUpYears);
                _logger.LogInformation("Ecospace spin-up = {SpinUpYears} years", ds.SpinUpYears);
            }

            if (mod.MyRunModel.MinHabCap == 0)
            {
                Console.WriteLine("Ecospace habitat corrections = off");
                _logger.LogInformation("Ecospace habitat corrections = off");
            }
            ds.UseHabCapGradientCorrections = false;
            if (mod.MyRunModel.MinHabCap > 0)
            {
                ds.UseHabCapGradientCorrections = true;
                ds.MinHabCap = mod.MyRunModel.MinHabCap;
                Console.WriteLine("Ecospace habitat corrections = {0}", ds.MinHabCap);
                _logger.LogInformation("Ecospace habitat corrections = {MinHabCap}", ds.MinHabCap);
            }

            if (mod.MyRunModel.MaxCores > 0)
            {
                ds.nEffortDistThreads = mod.MyRunModel.MaxCores;
                ds.nSpaceSolverThreads = mod.MyRunModel.MaxCores;
                ds.nGridSolverThreads = mod.MyRunModel.MaxCores;
                ds.nIBMMovementSolverThreads = mod.MyRunModel.MaxCores;
                Console.WriteLine("Ecospace #threads = {0}", ds.nEffortDistThreads);
                _logger.LogInformation("Ecospace #threads = {Threads}", ds.nEffortDistThreads);
            }

            ds.UseIBM = mod.MyRunModel.UseIBM;
            ds.NewMultiStanza = !ds.UseIBM;
            Console.WriteLine("Ecospace runmode = {0}", ds.UseIBM ? "ibm" : "multi-stanza");
            _logger.LogInformation("Ecospace runmode = {RunMode}", ds.UseIBM ? "ibm" : "multi-stanza");

            ConfigureAutosave(mod);

            // Check data connections
            Console.WriteLine("Ecospace no. ext data connections = {0}", Core.SpatialDataConnectionManager.NumConnectedAdapters);
            Console.WriteLine();
            Console.WriteLine("Start run");
            _logger.LogInformation("Ecospace start run");

            // Wire bridge callback as a lambda that captures mod and the local success flag
            IPlugin? pi = GetPlugin(typeof(cEcospaceBridgePlugin));
            if (pi != null)
            {
                cEcospaceBridgePlugin ppt = (cEcospaceBridgePlugin)pi;
                ppt.BridgeCallback = (cEcospaceBridgePlugin.EventType e, int iTime) =>
                {
                    if (e == cEcospaceBridgePlugin.EventType.BeginTimeStep)
                    {
                        // Print out time tracking
                        if ((iTime - 1) % cCore.N_MONTHS == 0)
                        {
                            cEcospaceDataStructures stepDs = Core.EcospaceDataStructures;
                            int year = Core.EcospaceTimestepToAbsoluteTime(iTime).Year;
                            string spinTag = stepDs.bInSpinUp ? " (spin-up)" : "";
                            string saveTag = (iTime == stepDs.FirstOutputTimeStep) ? " autosaving starting" : "";
                            Console.WriteLine("{0}{1}{2}", year, spinTag, saveTag);
                            _logger.LogInformation("Ecospace year {Year}{SpinTag}{SaveTag}", year, spinTag, saveTag);
                        }
                        runSuccess &= Apply(mod, iTime);
                    }
                };
            }

            // Capture Ecospace STDF messages
            cMessageHandler mh = new cMessageHandler(
                (ref cMessage msg) =>
                {
                    if (msg.Importance == eMessageImportance.Information)
                    {
                        Console.WriteLine("STDF: {0}", msg.Message);
                        _logger.LogInformation("STDF: {Message}", msg.Message);
                    }
                    if (msg.Importance == eMessageImportance.Critical)
                    {
                        Console.WriteLine("! STDF: {0}", msg.Message);
                        _logger.LogWarning("STDF: {Message}", msg.Message);
                    }
                },
                eCoreComponentType.External,
                eMessageType.GISOperation,
                new SynchronizationContext());
            Core.Messages.AddMessageHandler(mh);

            // Go for it - added delegate to meet the requirements of the API
            cCore.EcoSpaceInterfaceDelegate dgt = new cCore.EcoSpaceInterfaceDelegate(
                (ref cEcospaceTimestep spaceres) => { /* NOP */ });
            runSuccess &= Core.RunEcospace(ref dgt, false);

            Console.WriteLine("End run");
            _logger.LogInformation("Ecospace end run");

            // Clean up
            Core.Messages.RemoveMessageHandler(mh);

            return runSuccess;
        }

        #region Internals

        // --------------------------------------------------------------------
        /// <summary>
        /// Configure Ecospace result writers and set the first output time step.
        /// </summary>
        // --------------------------------------------------------------------
        private void ConfigureAutosave(cEcospaceModifier mod)
        {
            cEcospaceModelParameters parms = Core.EcospaceModelParameters;
            cEcospaceDataStructures ds = Core.EcospaceDataStructures;

            Console.WriteLine("Ecospace {0} result writer(s)", parms.nResultWriters);
            for (int i = 0; i < parms.nResultWriters; i++)
            {
                bool bEnable = false;

                IEcospaceResultsWriter wr = parms.ResultWriter(i + 1);
                string n = wr.GetType().Name.ToLower();
                if (mod.IsASCWriter(n))
                {
                    foreach (var vn in mod.MyRunModel.SaveContentASC)
                    {
                        if (n.Contains(vn.ToLower()))
                            bEnable = true;
                    }
                }
                else if (mod.IsCSVWriter(n))
                {
                    foreach (var vn in mod.MyRunModel.SaveContentCSV)
                    {
                        if (n.Contains(vn.ToLower()))
                            bEnable = true;
                    }
                }
                wr.Enabled = bEnable;
                Console.WriteLine("  {0,2}: {1} {2}", i, wr.Enabled ? "V" : "-", wr.GetType().ToString());
            }

            // Set autosave properties
            ds.FirstOutputTimeStep = DateToTimeStep(new DateTime(Math.Max(Core.EcosimFirstYear(), mod.MyRunModel.SaveFirstYear), 1, 1));
            ds.SaveAnnual = mod.MyRunModel.SaveAnnual;
            Console.WriteLine("Ecospace writing output at time step {0}, {1}", ds.FirstOutputTimeStep, ds.SaveAnnual ? "annual" : "monthly");
        }

        #endregion // Internals
    }
}

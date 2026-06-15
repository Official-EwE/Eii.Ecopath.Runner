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
    public class cEcospaceModifierService
    {
        #region Private vars

        private readonly ILogger<cEcospaceModifierService> _logger;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcospaceModifierService(ILogger<cEcospaceModifierService> logger)
        {
            _logger = logger;
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

            cEcospaceDataStructures ds = mod.Core.EcospaceDataStructures;

            // Disable unnecessary data caching - Ecospace won't be saved after
            ds.PreserveLayerData = false;
            // Reroute spatial log folder
            mod.Core.SpatialOperationLog.LogFilePath = mod.Core.OutputPath;

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

            // Check data connections
            Console.WriteLine("Ecospace no. ext data connections = {0}", mod.Core.SpatialDataConnectionManager.NumConnectedAdapters);
            Console.WriteLine();
            Console.WriteLine("Start run");
            _logger.LogInformation("Ecospace start run");

            // Wire bridge callback as a lambda that captures mod and the local success flag
            IPlugin? pi = mod.GetPlugin(typeof(cEcospaceBridgePlugin));
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
                            cEcospaceDataStructures stepDs = mod.Core.EcospaceDataStructures;
                            int year = mod.Core.EcospaceTimestepToAbsoluteTime(iTime).Year;
                            string spinTag = stepDs.bInSpinUp ? " (spin-up)" : "";
                            string saveTag = (iTime == stepDs.FirstOutputTimeStep) ? " autosaving starting" : "";
                            Console.WriteLine("{0}{1}{2}", year, spinTag, saveTag);
                            _logger.LogInformation("Ecospace year {Year}{SpinTag}{SaveTag}", year, spinTag, saveTag);
                        }
                        runSuccess &= mod.Apply(iTime);
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
            mod.Core.Messages.AddMessageHandler(mh);

            // Go for it - added delegate to meet the requirements of the API
            cCore.EcoSpaceInterfaceDelegate dgt = new cCore.EcoSpaceInterfaceDelegate(
                (ref cEcospaceTimestep spaceres) => { /* NOP */ });
            runSuccess &= mod.Core.RunEcospace(ref dgt, false);

            Console.WriteLine("End run");
            _logger.LogInformation("Ecospace end run");

            // Clean up
            mod.Core.Messages.RemoveMessageHandler(mh);

            return runSuccess;
        }
    }
}

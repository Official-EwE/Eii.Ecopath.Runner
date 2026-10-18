using Eii.Ecopath.Runner.Datamodel.Automation;
using EwEBridge.Ecosim;
using EwECore;
using EwECore.Ecosim;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Service responsible for executing an Ecosim run using a configured
    /// <see cref="cEcosimModifier"/>.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cEcosimModifierService : cRuntimeModifierService
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcosimModifierService(ICoreService coreService, cNodeService nodeService, ILogger<cEcosimModifierService> logger)
            : base(coreService, nodeService, logger)
        {
        }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        protected override int DateToTimeStep(DateTime date)
        {
            return _coreService.AbsoluteTimeToEcosimTimestep(date);
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Execute the Ecosim run described by <paramref name="mod"/>.
        /// </summary>
        /// <param name="mod">A fully configured <see cref="cEcosimModifier"/>.</param>
        /// <returns>True if the run completed without errors.</returns>
        // --------------------------------------------------------------------
        internal bool Run(cEcosimModifier mod)
        {
            bool runSuccess = true;

            CompleteAndPrepareChanges(mod);
            ConfigureAutosave(mod, out List<cEcosimResultWriter.eResultTypes> autosaveResults, out bool bSaveAnnual);

            // Execute the first time step BEFORE the core is busy with the run, so that we can capture the initial state of the model before any changes are applied.
            runSuccess &= Apply(mod, 1);

            // Wire bridge callback as a lambda that captures mod and the local success flag
            IPlugin? pi = GetPlugin(typeof(cEcosimBridgePlugin));
            if (pi != null)
            {
                cEcosimBridgePlugin ppt = (cEcosimBridgePlugin)pi;
                ppt.BridgeCallback = (cEcosimBridgePlugin.EventType e, int iTime) =>
                {
                    if (e == cEcosimBridgePlugin.EventType.BeginTimeStep)
                    {
                        cEcosimDatastructures ds = _coreService.EcosimDataStructures;

                        // Print out time tracking
                        if ((iTime - 1) % ds.NumStepsPerYear == 0)
                        {
                            int year = (int)_coreService.EcosimFirstYear() + ((iTime - 1) / ds.NumStepsPerYear);
                            Console.WriteLine("{0}", year);
                            _logger.LogInformation("Ecosim year {Year}", year);
                        }
                        if (iTime > 1)
                            runSuccess &= Apply(mod, iTime);
                    }
                };
            }

            // Go for it
            runSuccess &= _coreService.RunEcosim();
            DoAutosave(autosaveResults, bSaveAnnual);

            return runSuccess;
        }

        #region Internals

        // --------------------------------------------------------------------
        /// <summary>
        /// Determine which result types to auto-save and whether to save annually.
        /// </summary>
        // --------------------------------------------------------------------
        private void ConfigureAutosave(cEcosimModifier mod,
            out List<cEcosimResultWriter.eResultTypes> autosaveResults,
            out bool bSaveAnnual)
        {
            autosaveResults = [];
            bSaveAnnual = mod.MyRunModel.SaveAnnual;

            if (mod.MyRunModel.SaveContentCSV != null)
            {
                bool isValid = true;

                Console.WriteLine("Configuring Ecosim autosaving");
                Console.WriteLine("* Ecosim writing output {0}", bSaveAnnual ? "annual" : "monthly");
                foreach (string s in mod.MyRunModel.SaveContentCSV)
                {
                    if (!Enum.TryParse(typeof(cEcosimResultWriter.eResultTypes), s, true, out object? parsed))
                    {
                        isValid = false;
                        Console.WriteLine($"x Unrecognized Ecosim output type '{s}'");
                    }
                    else
                    {
                        autosaveResults.Add((cEcosimResultWriter.eResultTypes)parsed);
                    }
                }
                if (!isValid)
                {
                    Console.WriteLine("Supported Ecosim ouput types are:");
                    foreach (string name in Enum.GetNames(typeof(cEcosimResultWriter.eResultTypes)))
                        Console.WriteLine($"  - {name}");
                }
            }
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Write Ecosim results after the run has completed.
        /// </summary>
        // --------------------------------------------------------------------
        private void DoAutosave(List<cEcosimResultWriter.eResultTypes> autosaveResults, bool bSaveAnnual)
        {
            cEcosimResultWriter wr = new cEcosimResultWriter(_coreService.Core);
            if (autosaveResults.Count > 0)
            {
                string path = _coreService.get_DefaultOutputPath(eAutosaveTypes.EcosimResults);
                wr.WriteResults(path, autosaveResults.ToArray(), bSaveAnnual ? eAutosaveFrequency.Annually : eAutosaveFrequency.Monthly, false);
                Console.WriteLine("Ecosim wrote output to {0}", path);
            }
        }

        #endregion // Internals
    }
}

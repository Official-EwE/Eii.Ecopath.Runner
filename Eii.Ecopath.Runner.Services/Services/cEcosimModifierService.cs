using Eii.Ecopath.Runner.Datamodel.Automation;
using EwEBridge.Ecosim;
using EwECore;
using EwECore.Ecosim;
using EwECore.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

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
            bSaveAnnual = false;

            if (mod.MyRunModel.SaveContentCSV != null)
            {
                string requests = string.Join(" ", mod.MyRunModel.SaveContentCSV.ToArray()).ToLower();
                foreach (cEcosimResultWriter.eResultTypes result in (cEcosimResultWriter.eResultTypes[])Enum.GetValues(typeof(cEcosimResultWriter.eResultTypes)))
                {
                    if (requests.Contains(result.ToString().ToLower()))
                        autosaveResults.Add(result);
                }
                bSaveAnnual = mod.MyRunModel.SaveAnnual;
                Console.WriteLine("Ecosim writing output {0}", bSaveAnnual ? "annual" : "monthly");
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
                wr.WriteResults(path, null, bSaveAnnual ? TriState.False : TriState.True, false);
                Console.WriteLine("Ecosim wrote output to {0}", path);
            }
        }

        #endregion // Internals
    }
}

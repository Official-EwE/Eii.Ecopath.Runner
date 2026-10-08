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
                bool success = wr.WriteResults(path, autosaveResults.ToArray(), bSaveAnnual ? TriState.False : TriState.True, false);
                RelocateLowerCasedOutput(path, _logger);
                Console.WriteLine("Ecosim wrote {0} result type(s) to {1}. Success: {2}", autosaveResults.Count, path, success);
            }
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Workaround for EwECore <c>cEcosimResultWriter.GetOutputFileName</c>, which
        /// lower-cases the entire output path instead of only the file name. On
        /// case-sensitive file systems (Linux) this writes CSV files into an
        /// all-lowercase twin of <paramref name="path"/>. This method moves those
        /// files back into <paramref name="path"/> and removes the empty twin.
        /// </summary>
        /// <remarks>
        /// ToDo: remove once EwECore only lower-cases the file name.
        /// </remarks>
        // --------------------------------------------------------------------
        internal static void RelocateLowerCasedOutput(string path, ILogger logger)
        {
            if (OperatingSystem.IsWindows() || string.IsNullOrEmpty(path))
                return;

            string lowered = path.ToLowerInvariant();
            if (lowered == path || !Directory.Exists(lowered))
                return;

            string fullPath = Path.GetFullPath(path);
            string fullLowered = Path.GetFullPath(lowered);
            if (fullPath == fullLowered)
                return;

            try
            {
                Directory.CreateDirectory(fullPath);

                int nMoved = 0;
                foreach (string file in Directory.GetFiles(fullLowered))
                {
                    File.Move(file, Path.Combine(fullPath, Path.GetFileName(file)), true);
                    nMoved++;
                }

                // Remove the now-empty lower-cased twin folders, walking upward while
                // they are empty and do not coincide with the original path's ancestors.
                string? dirLowered = fullLowered;
                string? dirOriginal = fullPath;
                while (dirLowered != null && dirOriginal != null &&
                       dirLowered != dirOriginal &&
                       Directory.Exists(dirLowered) &&
                       !Directory.EnumerateFileSystemEntries(dirLowered).Any())
                {
                    Directory.Delete(dirLowered);
                    dirLowered = Path.GetDirectoryName(dirLowered);
                    dirOriginal = Path.GetDirectoryName(dirOriginal);
                }

                logger.LogInformation("Relocated {Count} Ecosim output file(s) from '{Lowered}' to '{Path}'", nMoved, fullLowered, fullPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to relocate Ecosim output from '{Lowered}' to '{Path}'", fullLowered, fullPath);
            }
        }

        #endregion // Internals
    }
}

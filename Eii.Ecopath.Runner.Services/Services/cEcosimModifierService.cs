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
    public class cEcosimModifierService
    {
        #region Private vars

        private readonly ILogger<cEcosimModifierService> _logger;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcosimModifierService(ILogger<cEcosimModifierService> logger)
        {
            _logger = logger;
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

            // Wire bridge callback as a lambda that captures mod and the local success flag
            IPlugin? pi = mod.GetPlugin(typeof(cEcosimBridgePlugin));
            if (pi != null)
            {
                cEcosimBridgePlugin ppt = (cEcosimBridgePlugin)pi;
                ppt.BridgeCallback = (cEcosimBridgePlugin.EventType e, int iTime) =>
                {
                    if (e == cEcosimBridgePlugin.EventType.BeginTimeStep)
                    {
                        cEcosimDatastructures ds = mod.Core.EcosimDataStructures;

                        // Print out time tracking
                        if ((iTime - 1) % ds.NumStepsPerYear == 0)
                        {
                            int year = (int)mod.Core.EcosimFirstYear() + ((iTime - 1) / ds.NumStepsPerYear);
                            Console.WriteLine("{0}", year);
                            _logger.LogInformation("Ecosim year {Year}", year);
                        }
                        runSuccess &= mod.Apply(iTime);
                    }
                };
            }

            // Go for it
            runSuccess &= mod.Core.RunEcosim();
            mod.DoAutosave();

            return runSuccess;
        }
    }
}

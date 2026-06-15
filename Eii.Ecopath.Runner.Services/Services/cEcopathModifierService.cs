using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Service responsible for executing an Ecopath run using a configured
    /// <see cref="cEcopathModifier"/>.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cEcopathModifierService
    {
        #region Private vars

        private readonly ILogger<cEcopathModifierService> _logger;

        #endregion // Private vars

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcopathModifierService(ILogger<cEcopathModifierService> logger)
        {
            _logger = logger;
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Execute the Ecopath run described by <paramref name="mod"/>.
        /// </summary>
        /// <param name="mod">A fully configured <see cref="cEcopathModifier"/>.</param>
        /// <returns>True if the run completed without errors.</returns>
        // --------------------------------------------------------------------
        internal bool Run(cEcopathModifier mod)
        {
            bool runSuccess = mod.Apply(cRuntimeModifier.FirstTimeStep);
            runSuccess &= mod.Core.RunEcopath();
            return runSuccess;
        }
    }
}

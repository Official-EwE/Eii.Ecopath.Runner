using Eii.Ecopath.Runner.Datamodel.Automation;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Service responsible for executing an Ecopath run using a configured
    /// <see cref="cEcopathModifier"/>.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cEcopathModifierService : cRuntimeModifierService
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcopathModifierService(IcCoreService coreService, cNodeService nodeService, ILogger<cEcopathModifierService> logger)
            : base(coreService, nodeService, logger)
        {
        }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        protected override int DateToTimeStep(DateTime date)
        {
            return cRuntimeModifier.FirstTimeStep; // Bwahah!
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
            CompleteAndPrepareChanges(mod);
            // ToDo: configure Ecopath settings
            bool runSuccess = Apply(mod, cRuntimeModifier.FirstTimeStep);
            runSuccess &= _coreService.RunEcopath();
            return runSuccess;
        }
    }
}

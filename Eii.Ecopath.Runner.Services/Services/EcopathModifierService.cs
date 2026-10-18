using Eii.Ecopath.Runner.Datamodel.Automation;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Service responsible for executing an Ecopath run using a configured
    /// <see cref="EcopathModifier"/>.
    /// </summary>
    // ------------------------------------------------------------------------
    public class EcopathModifierService : RuntimeModifierService
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        // --------------------------------------------------------------------
        public EcopathModifierService(ICoreService coreService, NodeService nodeService, ILogger<EcopathModifierService> logger)
            : base(coreService, nodeService, logger)
        {
        }

        // --------------------------------------------------------------------
        /// <inheritdoc/>
        // --------------------------------------------------------------------
        protected override int DateToTimeStep(DateTime date)
        {
            return RuntimeModifier.FirstTimeStep; // Bwahah!
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// Execute the Ecopath run described by <paramref name="mod"/>.
        /// </summary>
        /// <param name="mod">A fully configured <see cref="EcopathModifier"/>.</param>
        /// <returns>True if the run completed without errors.</returns>
        // --------------------------------------------------------------------
        internal bool Run(EcopathModifier mod)
        {
            CompleteAndPrepareChanges(mod);

            // ToDo: configure Ecopath settings
            bool runSuccess = Apply(mod, RuntimeModifier.FirstTimeStep);
            runSuccess &= _coreService.RunEcopath();

            if (runSuccess)
            {
                if (mod.MyRunModel.SaveContentCSV != null && mod.MyRunModel.SaveContentCSV.Count > 0)
                {
                    cEcopathResultWriter wr = new cEcopathResultWriter(_coreService.Core);
                    wr.WriteResults(true);
                }
            }
            return runSuccess;
        }
    }
}

using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Runtime
{
    internal class cEcopathModifier : cRuntimeModifier
    {
        public cEcopathModifier(cCore core, cEwEConfiguration config, IModelRunInstructions runmodel, cNodeService nodeService, ILogger<cEcopathModifier> logger)
            : base(core, "ecopath", config, runmodel, nodeService, logger)
        {
            // ToDo: configure Ecopath settings
        }

        protected override int DateToTimeStep(DateTime date)
        {
            return FirstTimeStep; // Bwahah!
        }

        public override void ConfigureAutosave()
        {
            // ToDo
        }
    }
}
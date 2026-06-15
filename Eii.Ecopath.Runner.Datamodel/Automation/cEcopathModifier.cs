using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    public class cEcopathModifier : cRuntimeModifier
    {
        public cEcopathModifier(cEwEConfiguration config, IModelRunInstructions runmodel)
            : base("ecopath", config, runmodel)
        {
        }
    }
}

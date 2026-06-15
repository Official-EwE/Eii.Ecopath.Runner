using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    public class cEcosimModifier : cRuntimeModifier
    {
        public cEcosimModifier(cEwEConfiguration config, cEcosimRunInstructions runmodel)
            : base("ecosim", config, runmodel)
        {
        }

        public cEcosimRunInstructions MyRunModel => (cEcosimRunInstructions)RunModel;
    }
}

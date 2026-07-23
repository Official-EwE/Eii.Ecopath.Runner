using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceGroupNicheNode : cEwECoreNode
    {
        public cEcospaceGroupNicheNode(ICoreService coreService, cEcospaceGroupInput group, ILogger logger) : base(coreService, group, logger)
        {
        }

        protected cEcospaceGroupInput Group => (cEcospaceGroupInput)this.CoreObj;

        [Description("Get the base capacity map (niche map) of this group")]
        public cMapNode map()
        {
            var bm = Core.EcospaceBasemap;
            return new cMapNode(CoreService, bm.get_LayerHabitatCapacityInput(CoreObj.Index), Logger);
        }

        [Description("Set whether to include habitats when calculation foraging capacity for this group")]
        public void usehabitats(bool flag)
        {
            if (flag)
                this.Group.CapacityCalculationType |= eEcospaceCapacityCalType.Habitat;
            else
                this.Group.CapacityCalculationType &= ~eEcospaceCapacityCalType.Habitat;

        }


        [Description("Set whether to include environmental drivers when calculation foraging capacity for this group")]
        public void useenvdrivers(bool flag)
        {
            if (flag)
                this.Group.CapacityCalculationType |= eEcospaceCapacityCalType.EnvResponses;
            else
                this.Group.CapacityCalculationType &= ~eEcospaceCapacityCalType.EnvResponses;

        }
    }
}

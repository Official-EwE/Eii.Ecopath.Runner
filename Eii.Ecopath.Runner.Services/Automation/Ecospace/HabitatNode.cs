using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class HabitatNode : EwECoreNode
    {
        public HabitatNode(ICoreService coreService, cEcospaceHabitat hab, ILogger logger) : base(coreService, hab, logger)
        {
        }

        protected cEcospaceHabitat Habitat => (cEcospaceHabitat)this.CoreObj;

        // Accessor
        [Description("Access the habitat map layer")]
        public MapNode map()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new MapNode(CoreService, bm.get_LayerHabitat(this.CoreObj.Index), Logger);
        }

    }
}

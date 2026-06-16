using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cHabitatNode : cEwECoreNode
    {
        public cHabitatNode(ICoreService coreService, cEcospaceHabitat hab, ILogger logger): base(coreService, hab, logger)
        {
        }
        protected cEcospaceHabitat Habitat => (cEcospaceHabitat)this.CoreObj;

        // Accessor
        public cMapNode map()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new cMapNode(CoreService, bm.get_LayerHabitat(this.CoreObj.Index), Logger);
        }

    }
}

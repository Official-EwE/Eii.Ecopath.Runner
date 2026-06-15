using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cHabitatNode : cEwECoreNode
    {
        public cHabitatNode(cCore core, cEcospaceHabitat hab, ILogger logger): base(core, hab, logger)
        {
        }
        protected cEcospaceHabitat Habitat => (cEcospaceHabitat)this.CoreObj;

        // Accessor
        public cMapNode map()
        {
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cMapNode(this.Core, bm.get_LayerHabitat(this.CoreObj.Index), Logger);
        }

    }
}

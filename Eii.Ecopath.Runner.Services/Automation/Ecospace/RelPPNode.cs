using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class RelPPNode : EwECoreNode
    {
        public RelPPNode(ICoreService coreService, ILogger logger) : base(coreService, coreService.EcospaceBasemap.LayerRelPP, logger)
        {
        }

        protected cEcospaceLayerRelPP RelPPLayer => (cEcospaceLayerRelPP)this.CoreObj;

        // Accessor
        [Description("Access the relative PP layer")]
        public MapNode map()
        {
            return new MapNode(CoreService, RelPPLayer, Logger);
        }

    }
}

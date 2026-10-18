using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class MPANode : EwECoreNode
    {
        public MPANode(ICoreService coreService, cEcospaceMPA mpa, ILogger logger) : base(coreService, mpa, logger) 
        {
        }

        protected cEcospaceMPA MPA => (cEcospaceMPA)this.CoreObj;
        // Accessor
        [Description("Access the MPA boundary map layer")]
        public MapNode map()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new MapNode(CoreService, bm.get_LayerMPA(this.CoreObj.Index), Logger);
        }

    }
}

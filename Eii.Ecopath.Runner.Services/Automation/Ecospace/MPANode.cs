using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cMPANode : cEwECoreNode
    {
        public cMPANode(ICoreService coreService, cEcospaceMPA mpa, ILogger logger) : base(coreService, mpa, logger) 
        {
        }

        protected cEcospaceMPA MPA => (cEcospaceMPA)this.CoreObj;
        // Accessor
        [Description("Access the MPA boundary map layer")]
        public cMapNode map()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new cMapNode(CoreService, bm.get_LayerMPA(this.CoreObj.Index), Logger);
        }

    }
}

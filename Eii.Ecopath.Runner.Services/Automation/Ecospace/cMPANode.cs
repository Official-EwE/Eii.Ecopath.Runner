using Eii.Ecopath.Runner.Services.Automation;
using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace EwERuEii.Ecopath.Runner.ServicesnConsole.Automation
{
    public class cMPANode : cEwECoreNode
    {
        public cMPANode(IcCoreService coreService, cEcospaceMPA mpa, ILogger logger) : base(coreService, mpa, logger) 
        {
        }

        protected cEcospaceMPA MPA => (cEcospaceMPA)this.CoreObj;
        // Accessor
        public cMapNode map()
        {
            cEcospaceBasemap bm = CoreService.EcospaceBasemap;
            return new cMapNode(CoreService, bm.get_LayerMPA(this.CoreObj.Index), Logger);
        }

    }
}

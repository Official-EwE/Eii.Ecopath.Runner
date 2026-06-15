using Eii.Ecopath.Runner.Services.Automation;
using EwECore;
using Microsoft.Extensions.Logging;

namespace EwERuEii.Ecopath.Runner.ServicesnConsole.Automation
{
    public class cMPANode : cEwECoreNode
    {
        public cMPANode(cCore core, cEcospaceMPA mpa, ILogger logger) : base(core, mpa, logger) 
        {
        }

        protected cEcospaceMPA MPA => (cEcospaceMPA)this.CoreObj;
        // Accessor
        public cMapNode map()
        {
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cMapNode(this.Core, bm.get_LayerMPA(this.CoreObj.Index), Logger);
        }

    }
}

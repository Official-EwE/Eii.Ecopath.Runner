using EwECore;

namespace EwERunConsole.Automation
{
    public class cMPANode : cEwECoreNode
    {
        public cMPANode(cCore core, cEcospaceMPA mpa) : base(core, mpa) 
        {
        }

        protected cEcospaceMPA MPA => (cEcospaceMPA)this.CoreObj;
        // Accessor
        public cMapNode map()
        {
            cEcospaceBasemap bm = this.Core.EcospaceBasemap;
            return new cMapNode(this.Core, bm.get_LayerMPA(this.CoreObj.Index));
        }

    }
}

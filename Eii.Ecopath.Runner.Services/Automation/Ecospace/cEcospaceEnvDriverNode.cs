using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceEnvDriverNode : cEwECoreNode
    {
        public cEcospaceEnvDriverNode(cCore core,  cEcospaceLayer driver) : base(core, driver) 
        {
        }

        protected cEcospaceLayer Driver => (cEcospaceLayer)this.CoreObj;

        // Accessor
        public cMapNode map()
        {
            return new cMapNode(this.Core, Driver);
        }
    }
}

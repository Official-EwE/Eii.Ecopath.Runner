using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceEnvDriverNode : cEwECoreNode
    {
        public cEcospaceEnvDriverNode(cCore core, cEcospaceLayer driver, ILogger logger) : base(core, driver, logger) 
        {
        }

        protected cEcospaceLayer Driver => (cEcospaceLayer)this.CoreObj;

        // Accessor
        public cMapNode map()
        {
            return new cMapNode(this.Core, Driver, Logger);
        }
    }
}

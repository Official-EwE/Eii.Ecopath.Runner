using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceEnvDriverNode : cEwECoreNode
    {
        public cEcospaceEnvDriverNode(ICoreService coreService, cEcospaceLayer driver, ILogger logger) : base(coreService, driver, logger) 
        {
        }

        protected cEcospaceLayer Driver => (cEcospaceLayer)this.CoreObj;

        // Accessor
        [Description("Access the environmental driver map layer")]
        public cMapNode map()
        {
            return new cMapNode(CoreService, Driver, Logger);
        }
    }
}

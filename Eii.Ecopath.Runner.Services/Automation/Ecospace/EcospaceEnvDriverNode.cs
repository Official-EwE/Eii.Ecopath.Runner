using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class EcospaceEnvDriverNode : EwECoreNode
    {
        public EcospaceEnvDriverNode(ICoreService coreService, cEcospaceLayer driver, ILogger logger) : base(coreService, driver, logger)
        {
        }

        protected cEcospaceLayer Driver => (cEcospaceLayer)this.CoreObj;

        // Accessor
        [Description("Access the environmental driver map layer")]
        public MapNode map()
        {
            return new MapNode(CoreService, Driver, Logger);
        }

        [Description("Set whether to include this env driver in the habitat foraging calculations")]
        public void drivecapacity(bool flag)
        {
            if (Driver.GetType() == typeof(cEcospaceLayerDepth))
                ((cEcospaceLayerDepth)Driver).IsCapacityEnabled = flag;
            else
                ((cEcospaceLayerDriver)Driver).IsCapacityEnabled = flag;
        }
    }
}

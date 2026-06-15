using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceGroupNode : cEwECoreNode
    {
        public cEcospaceGroupNode(cCore core, cEcospaceGroupInput group, ILogger logger) : base(core, group, logger)
        { 
        }
        protected cEcospaceGroupInput Group => (cEcospaceGroupInput)this.CoreObj;

        /// <summary>
        /// Facade
        /// </summary>
        /// <param name="mvel"></param>
        /// <returns></returns>
        public bool mvel(float mvel)
        {
            return dispersal_rate(mvel);
        }

        public bool dispersal_rate(float mvel)
        {
            return this.SetVariable(eVarNameFlags.MVel, mvel);
        }
    }
}

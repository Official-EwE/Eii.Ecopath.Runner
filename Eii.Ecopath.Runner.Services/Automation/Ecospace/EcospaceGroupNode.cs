using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class EcospaceGroupNode : EwECoreNode
    {
        public EcospaceGroupNode(ICoreService coreService, cEcospaceGroupInput group, ILogger logger) : base(coreService, group, logger)
        {
        }
        protected cEcospaceGroupInput Group => (cEcospaceGroupInput)this.CoreObj;

        /// <summary>
        /// Facade
        /// </summary>
        /// <param name="mvel"></param>
        /// <returns></returns>
        [Description("Set the dispersal rate (alias for dispersal_rate)")]
        public bool mvel(float mvel)
        {
            return dispersal_rate(mvel);
        }

        [Description("Set the group's dispersal rate (MVel)")]
        public bool dispersal_rate(float mvel)
        {
            return this.SetVariable(eVarNameFlags.MVel, mvel);
        }

        [Description("Access the niche data of this group")]
        public EcospaceGroupNicheNode niche()
        {
            return new EcospaceGroupNicheNode(CoreService, Group, Logger);
        }
    }
}

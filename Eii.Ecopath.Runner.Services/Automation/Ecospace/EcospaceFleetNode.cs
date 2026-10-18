using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceFleetNode : cEwECoreNode
    {

        public cEcospaceFleetNode(ICoreService coreService, cEcospaceFleetInput fleet, ILogger logger) : base(coreService, fleet, logger)
        { 
        }
        
        protected cEcospaceFleetInput Fleet => (cEcospaceFleetInput)CoreObj;

        [Description("Set the fleet's effective power scaling")]
        public bool effective_power(float em)
        {
            return this.SetVariable(eVarNameFlags.EffectivePower, em);
        }

        [Description("Set the fleet's spatial effort multiplier")]
        public bool semult(float semult)
        {
            return this.SetVariable(eVarNameFlags.SEmult, semult);
        }

        /// <summary>
        /// Bridge
        /// </summary>
        /// <param name="mult"></param>
        /// <returns></returns>
        [Description("Set the fleet's spatial effort multiplier (alias for semult)")]
        public bool effort_multiplier(float mult)
        {
            return this.semult(mult);
        }
    }
}

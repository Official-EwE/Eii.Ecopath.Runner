using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcospaceFleetNode : cEwECoreNode
    {

        public cEcospaceFleetNode(cCore core, cEcospaceFleetInput fleet) : base(core, fleet)
        { 
        }
        
        protected cEcospaceFleetInput Fleet => (cEcospaceFleetInput)CoreObj;

        public bool effective_power(float em)
        {
            return this.SetVariable(eVarNameFlags.EffectivePower, em);
        }

        public bool semult(float semult)
        {
            return this.SetVariable(eVarNameFlags.SEmult, semult);
        }

        /// <summary>
        /// Bridge
        /// </summary>
        /// <param name="mult"></param>
        /// <returns></returns>
        public bool effort_multiplier(float mult)
        {
            return this.semult(mult);
        }
    }
}

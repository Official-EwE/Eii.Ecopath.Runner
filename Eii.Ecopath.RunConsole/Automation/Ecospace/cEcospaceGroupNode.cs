using EwECore;

namespace EwERunConsole.Automation
{
    public class cEcospaceGroupNode : cEwECoreNode
    {
        public cEcospaceGroupNode(cCore core, cEcospaceGroupInput group) : base(core, group)
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

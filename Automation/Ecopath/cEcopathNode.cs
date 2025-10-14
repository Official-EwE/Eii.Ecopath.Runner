using EwECore;

namespace EwERunConsole.Automation
{
    public class cEcopathNode : cNode
    {
        public cEcopathNode(cCore core) : base(core) 
        {
        }

        public cEcopathGroupNode? group(int iGroup)
        {
            if ((iGroup < 1) | (iGroup > this.Core.nGroups))
            {
                Console.WriteLine("! Ecopath Group {0} invalid, must be [1, {1}]", iGroup, this.Core.nGroups);
                return null;
            }
            return new cEcopathGroupNode(this.Core, this.Core.get_EcopathGroupInputs(iGroup));
        }

        public cEcopathFleetNode? fleet(int iFleet)
        {
            if ((iFleet < 1) | (iFleet > this.Core.nFleets))
            {
                Console.WriteLine("! Ecopath Fleet {0} invalid, must be [1, {1}]", iFleet, this.Core.nFleets);
                return null;
            }
            return new cEcopathFleetNode(this.Core, this.Core.get_EcopathFleetInputs(iFleet));
        }
    }
}

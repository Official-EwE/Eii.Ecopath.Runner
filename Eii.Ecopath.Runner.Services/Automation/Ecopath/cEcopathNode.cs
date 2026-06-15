using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathNode : cNode
    {
        public cEcopathNode(cCore core, ILogger logger) : base(core, logger) 
        {
        }

        public cEcopathGroupNode? group(int iGroup)
        {
            if ((iGroup < 1) | (iGroup > this.Core.nGroups))
            {
                Logger.LogWarning("Ecopath Group {Group} invalid, must be [1, {Max}]", iGroup, this.Core.nGroups);
                return null;
            }
            return new cEcopathGroupNode(this.Core, this.Core.get_EcopathGroupInputs(iGroup), Logger);
        }

        public cEcopathFleetNode? fleet(int iFleet)
        {
            if ((iFleet < 1) | (iFleet > this.Core.nFleets))
            {
                Logger.LogWarning("Ecopath Fleet {Fleet} invalid, must be [1, {Max}]", iFleet, this.Core.nFleets);
                return null;
            }
            return new cEcopathFleetNode(this.Core, this.Core.get_EcopathFleetInputs(iFleet), Logger);
        }
    }
}

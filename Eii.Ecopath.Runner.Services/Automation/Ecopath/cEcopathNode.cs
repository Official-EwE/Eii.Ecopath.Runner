using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathNode : cNode
    {
        public cEcopathNode(ICoreService coreService, ILogger logger) : base(coreService, logger) 
        {
        }

        [Description("Select an Ecopath group by 1-based index")]
        public cEcopathGroupNode? group(int iGroup)
        {
            if ((iGroup < 1) | (iGroup > CoreService.nGroups))
            {
                Logger.LogWarning("Ecopath Group {Group} invalid, must be [1, {Max}]", iGroup, CoreService.nGroups);
                return null;
            }
            return new cEcopathGroupNode(CoreService, CoreService.get_EcopathGroupInputs(iGroup), Logger);
        }

        [Description("Select an Ecopath fleet by 1-based index")]
        public cEcopathFleetNode? fleet(int iFleet)
        {
            if ((iFleet < 1) | (iFleet > CoreService.nFleets))
            {
                Logger.LogWarning("Ecopath Fleet {Fleet} invalid, must be [1, {Max}]", iFleet, CoreService.nFleets);
                return null;
            }
            return new cEcopathFleetNode(CoreService, CoreService.get_EcopathFleetInputs(iFleet), Logger);
        }
    }
}

using Eii.Ecopath.Runner.Services.Runtime;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEcopathNode : cNode
    {
        public cEcopathNode(ICoreService coreService, ILogger logger) : base(coreService, logger)
        {
        }

        [Description("Select an Ecopath group by name")]
        public cEcopathGroupNode? group(string groupName)
        {
            int iGroup = FindGroup(groupName, Logger);
            if (iGroup < 0) return null;
            return new cEcopathGroupNode(CoreService, CoreService.get_EcopathGroupInputs(iGroup), Logger);
        }

        [Description("Select an Ecopath fleet by name")]
        public cEcopathFleetNode? fleet(string fleetName)
        {
            int iFleet = FindFleet(fleetName, Logger);
            if (iFleet < 0)
                return null;
            return new cEcopathFleetNode(CoreService, CoreService.get_EcopathFleetInputs(iFleet), Logger);
        }
    }
}

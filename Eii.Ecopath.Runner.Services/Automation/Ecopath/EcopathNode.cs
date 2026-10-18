using Eii.Ecopath.Runner.Services.Runtime;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class EcopathNode : Node
    {
        public EcopathNode(ICoreService coreService, ILogger logger) : base(coreService, logger)
        {
        }

        [Description("Select an Ecopath group by name")]
        public EcopathGroupNode? group(string groupName)
        {
            int iGroup = FindGroup(groupName, Logger);
            if (iGroup < 0) return null;
            return new EcopathGroupNode(CoreService, CoreService.get_EcopathGroupInputs(iGroup), Logger);
        }

        [Description("Select an Ecopath fleet by name")]
        public EcopathFleetNode? fleet(string fleetName)
        {
            int iFleet = FindFleet(fleetName, Logger);
            if (iFleet < 0)
                return null;
            return new EcopathFleetNode(CoreService, CoreService.get_EcopathFleetInputs(iFleet), Logger);
        }
    }
}

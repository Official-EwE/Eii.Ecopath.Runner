using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    /// <summary>
    /// Root class for building an EwE automation tree.
    /// </summary>
    public class cEwERootNode : cNode
    {
        public cEwERootNode(ICoreService coreService, ILogger logger) : base(coreService, logger) 
        { 
        }

        public cEcopathNode ecopath()
        {
            return new cEcopathNode(CoreService, Logger);
        }

        public cEcosimNode ecosim()
        {
            return new cEcosimNode(CoreService, CoreService.EcosimModelParameters, Logger);
        }

        public cEcospaceNode ecospace()
        {
            return new cEcospaceNode(CoreService, CoreService.EcospaceModelParameters, Logger);
        }

        [AutomationIgnore]
        public string[] AutomationTree()
        {
            return [.. ListAutomationTree()];
        }

        [AutomationIgnore]
        public string[] AutomationPaths()
        {
            return [.. ListAutomationPaths()];
        }
    }
}

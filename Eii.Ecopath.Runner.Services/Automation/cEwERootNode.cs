using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

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

        [Description("Enter the Ecopath modification context")]
        public cEcopathNode ecopath()
        {
            return new cEcopathNode(CoreService, Logger);
        }

        [Description("Enter the Ecosim modification context")]
        public cEcosimNode ecosim()
        {
            return new cEcosimNode(CoreService, CoreService.EcosimModelParameters, Logger);
        }

        [Description("Enter the Ecospace modification context")]
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

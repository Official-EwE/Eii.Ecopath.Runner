using Eii.Ecopath.Runner.Services.Runtime;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Eii.Ecopath.Runner.Services.Automation
{
    /// <summary>
    /// Root class for building an EwE automation tree.
    /// </summary>
    public class EwERootNode : Node
    {
        public EwERootNode(ICoreService coreService, ILogger logger) : base(coreService, logger)
        {
        }

        [Description("Enter the Ecopath modification context")]
        public EcopathNode ecopath()
        {
            return new EcopathNode(CoreService, Logger);
        }

        [Description("Enter the Ecosim modification context")]
        public EcosimNode ecosim()
        {
            return new EcosimNode(CoreService, CoreService.EcosimModelParameters, Logger);
        }

        [Description("Enter the Ecospace modification context")]
        public EcospaceNode ecospace()
        {
            return new EcospaceNode(CoreService, CoreService.EcospaceModelParameters, Logger);
        }

        [AutomationIgnore]
        public string[] AutomationPaths()
        {
            return [.. ListAutomationPaths()];
        }
    }
}

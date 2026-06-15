using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    /// <summary>
    /// Root class for building an EwE automation tree.
    /// </summary>
    public class cEwERootNode : cNode
    {
        public cEwERootNode(cCore core, ILogger logger) : base(core, logger) 
        { 
        }

        public cEcopathNode ecopath()
        {
            return new cEcopathNode(Core, Logger);
        }

        public cEcosimNode ecosim()
        {
            return new cEcosimNode(Core, Core.EcosimModelParameters, Logger);
        }

        public cEcospaceNode ecospace()
        {
            return new cEcospaceNode(Core, Core.EcospaceModelParameters, Logger);
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

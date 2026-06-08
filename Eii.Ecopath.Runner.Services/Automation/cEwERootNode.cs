using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    /// <summary>
    /// Root class for building an EwE automation tree.
    /// </summary>
    public class cEwERootNode : cNode
    {
        public cEwERootNode(cCore core) : base(core) 
        { 
        }

        public cEcopathNode ecopath()
        {
            return new cEcopathNode(Core);
        }

        public cEcosimNode ecosim()
        {
            return new cEcosimNode(Core, Core.EcosimModelParameters);
        }

        public cEcospaceNode ecospace()
        {
            return new cEcospaceNode(Core, Core.EcospaceModelParameters);
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

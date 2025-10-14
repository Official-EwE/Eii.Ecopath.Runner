using EwECore;

namespace EwERunConsole.Automation
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
            return new cEcopathNode(this.Core);
        }

        public cEcosimNode ecosim()
        {
            return new cEcosimNode(this.Core, this.Core.EcosimModelParameters);
        }

        public cEcospaceNode ecospace()
        {
            return new cEcospaceNode(this.Core, this.Core.EcospaceModelParameters);
        }

        [AutomationIgnore]
        public string[] AutomationTree()
        {
            return [.. this.ListAutomationTree()];
        }

        [AutomationIgnore]
        public string[] AutomationPaths()
        {
            return [.. this.ListAutomationPaths()];
        }
    }
}

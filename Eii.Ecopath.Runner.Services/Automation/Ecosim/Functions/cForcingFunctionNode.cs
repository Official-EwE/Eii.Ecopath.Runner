using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cForcingFunctionNode : cFunctionNode
    {
        public cForcingFunctionNode(cCore core, cShapeData shape, ILogger logger) : base(core, shape, logger)
        {
            // Nop
        }
    }
}

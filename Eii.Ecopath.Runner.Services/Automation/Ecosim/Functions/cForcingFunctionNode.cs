using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cForcingFunctionNode : cFunctionNode
    {
        public cForcingFunctionNode(ICoreService coreService, cShapeData shape, ILogger logger) : base(coreService, shape, logger)
        {
            // Nop
        }
    }
}

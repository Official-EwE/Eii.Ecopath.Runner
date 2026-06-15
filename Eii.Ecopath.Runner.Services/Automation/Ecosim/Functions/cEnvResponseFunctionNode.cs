using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEnvResponseFunctionNode : cFunctionNode
    {
        public cEnvResponseFunctionNode(IcCoreService coreService, cEnviroResponseFunction shapeData, ILogger logger) : base(coreService, shapeData, logger)
        {
        }

        protected cEnviroResponseFunction RespFn => (cEnviroResponseFunction)Shape;
    }
}

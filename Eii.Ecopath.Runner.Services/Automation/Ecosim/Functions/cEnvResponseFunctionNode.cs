using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEnvResponseFunctionNode : cFunctionNode
    {
        public cEnvResponseFunctionNode(cCore core, cEnviroResponseFunction shapeData, ILogger logger) : base(core, shapeData, logger)
        {
        }

        protected cEnviroResponseFunction RespFn => (cEnviroResponseFunction)Shape;
    }
}

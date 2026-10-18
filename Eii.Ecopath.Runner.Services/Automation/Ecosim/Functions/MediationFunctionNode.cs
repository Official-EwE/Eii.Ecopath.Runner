using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using Microsoft.Extensions.Logging;

namespace Eii.Ecopath.Runner.Services.Automation.Ecosim.Functions
{
    public class MediationFunctionNode : FunctionNode
    {
        public MediationFunctionNode(ICoreService coreService, cMediationFunction shapeData, ILogger logger) : base(coreService, shapeData, logger)
        {
        }

        protected cMediationFunction RespFn => (cMediationFunction)Shape;
    }
}

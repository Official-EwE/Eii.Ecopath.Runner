using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEnvResponseFunctionNode : cFunctionNode
    {
        public cEnvResponseFunctionNode(cCore core, cEnviroResponseFunction shapeData) : base(core, shapeData) 
        {
        }

        protected cEnviroResponseFunction RespFn => (cEnviroResponseFunction)Shape;
    }
}

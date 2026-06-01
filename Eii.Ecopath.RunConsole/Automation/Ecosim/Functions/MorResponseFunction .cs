using EwECore;

namespace EwERunConsole.Automation.Ecosim
{
    public class MortResponseFunction : Function
    {
        protected readonly cEnviroResponseFunction RespFn;

        public MortResponseFunction(cShapeData shapeData) : base(shapeData) 
        {
            this.RespFn = (cEnviroResponseFunction) shapeData;
        }

    }
}

using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    public class cEnvResponseFunctionNode : cFunctionNode
    {
        public cEnvResponseFunctionNode(cCore core, cEnviroResponseFunction shapeData) : base(core, shapeData)
        {
        }

        protected cEnviroResponseFunction RespFn => (cEnviroResponseFunction)Shape;

        public bool trapezoid(float leftbottom, float lefttop, float righttop, float rightbottom)
        {
            cTrapezoidShapeFunction fn = new cTrapezoidShapeFunction();
            this.Shape.LockUpdates();
            fn.LeftBottom = leftbottom;
            fn.LeftTop = lefttop;
            fn.RightTop = righttop;
            fn.RightBottom = rightbottom;
            this.RespFn.ResponseLeftLimit = leftbottom;
            this.RespFn.ResponseRightLimit = rightbottom;
            this.setpoints(fn.Shape(1200), false);
            this.Shape.UnlockUpdates();
            return true;
        }
    }
}

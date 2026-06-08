using EwECore;

namespace Eii.Ecopath.Runner.Services.Automation
{
    // --------------------------------------------------------------------
    /// <summary>
    /// A node in the automation tree that provides a number of operations
    /// on <see cref="cShapeData">functions</see>.
    /// </summary>
    // --------------------------------------------------------------------
    public abstract class cFunctionNode : cNode
    {
        protected readonly cShapeData Shape;

        public cFunctionNode(cCore core, cShapeData shape) :base(core)
        {
            this.Shape = shape;
        }

        // ----------------------------------------------------------------
        /// <summary>
        /// Load a function from a file. Not implemented yet.
        /// </summary>
        /// <param name="fn"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------
        public bool load(string fn)
        {

            this.Shape.Update();
            return false;
        }

        // ----------------------------------------------------------------
        /// <summary>
        /// Set the function from an array of points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------
        public bool reshape(float[] points)
        {
            this.Shape.LockUpdates();
            setpoints(points);
            this.Shape.UnlockUpdates();
            return true;
        }

        // ----------------------------------------------------------------
        /// <summary>
        /// Internal point setter function.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------
        protected bool setpoints(float[] points)
        {
            for (int i = 0; i<points.Length; i++)
                this.Shape.set_ShapeData(i, points[i]);
            return true;
        }
}
}

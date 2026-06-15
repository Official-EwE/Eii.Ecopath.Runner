using EwECore;
using EwECore.Common;

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

        public cFunctionNode(cCore core, cShapeData shape) : base(core)
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
        /// Set the function from an array of points, repeating the points 
        /// pattern to the end of the shape
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------
        public virtual bool fill(object[] points)
        {
            if (points == null) return false;
            var floatArray = points.Select(x => (float)Convert.ChangeType(x, typeof(float))).ToArray();
            this.Shape.LockUpdates();
            setpoints(floatArray, true);
            this.Shape.UnlockUpdates();
            return true;
        }

        public bool Reshape(string shapetypename, float[] parameters)
        {
            eShapeFunctionType shapetype = eShapeFunctionType.NotSet;

            // Parse shape shapetypename
            if (!Enum.TryParse(shapetypename, out shapetype))
            {
                // LOG THIS: Unable to parse function shapetypename 
                return false;
            }

            // Obtain primitive
            IShapeFunction fn = cShapeFunctionFactory.GetShapeFunction((long)shapetype, Core.PluginManager);
            if (fn == null)
            {
                // LOG THIS: Unable to get working shape type. Plugins may be absent
                return false;
            }

            // Is compatible?
            if (!fn.IsCompatible(this.Shape.DataType))
            {
                // LOG THIS: Shape is not compatible with provided primitive
                return false;
            }

            for (int i = 0; i < Math.Min(parameters.Count(), fn.nParameters))
                fn.set_ParamValue(i, parameters[i]);

            // Eeek
            fn.Apply(this.Shape);
            return true;
        }

        #region Internals

        // ----------------------------------------------------------------
        /// <summary>
        /// Internal point setter function.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------
        protected bool setpoints(float[] points, bool bRepeat)
        {
            if (points is null) return false;

            int n = points.Length;
            if (n == 0) return false;

            int iMax = bRepeat ? this.Shape.nPoints : Math.Min(n, this.Shape.nPoints);

            for (int i = 0; i < iMax; i++)
                this.Shape.set_ShapeData(i, points[i % n]);
            return true;
        }

        #endregion // Internal
    }
}
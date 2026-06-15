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
        /// Set the function from an array of points for as many point values
        /// that are provided.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        // ----------------------------------------------------------------
        public virtual bool set(object[] points)
        {
            if (points == null) return false;
            var floatArray = points.Select(x => (float)Convert.ChangeType(x, typeof(float))).ToArray();
            this.Shape.LockUpdates();
            setpoints(floatArray, false);
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
        protected bool setpoints(float[] points, bool repeat)
        {
            int imax = this.Shape.nPoints;
            int ilen = points.Length;
            int istep = 0;

            while (istep < imax && repeat ? true : istep < ilen)
            {
                this.Shape.set_ShapeData(istep, points[istep % ilen]);
                istep++;
            }

            return true;
        }
    }
}

using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using EwECore.Common;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

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

        public cFunctionNode(ICoreService coreService, cShapeData shape, ILogger logger) : base(coreService, logger)
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
        [Description("Load the function from a file (not yet implemented)")]
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
        [Description("Set function values from an array of points; excess points are ignored")]
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
        [Description("Set function values, repeating the pattern to fill the entire shape")]
        public virtual bool fill(object[] points)
        {
            if (points == null) return false;
            var floatArray = points.Select(x => (float)Convert.ChangeType(x, typeof(float))).ToArray();
            this.Shape.LockUpdates();
            setpoints(floatArray, true);
            this.Shape.UnlockUpdates();
            return true;
        }

        [Description("Apply a built-in shape primitive (e.g. Sigmoid, Normal) to the function")]
        public bool reshape(string shapetypename, float[] parameters)
        {
            eShapeFunctionType shapetype = eShapeFunctionType.NotSet;

            // Parse shape shapetypename
            if (!Enum.TryParse(shapetypename, out shapetype))
            {
                Logger.LogWarning("Unable to parse function shape type '{ShapeTypeName}'", shapetypename);
                return false;
            }

            // Obtain primitive
            IShapeFunction fn = cShapeFunctionFactory.GetShapeFunction((long)shapetype, CoreService.PluginManager);
            if (fn == null)
            {
                Logger.LogWarning("Unable to get shape function for type '{ShapeTypeName}'. Plugins may be absent", shapetypename);
                return false;
            }

            // Is compatible?
            if (!fn.IsCompatible(this.Shape.DataType))
            {
                Logger.LogWarning("Shape type '{ShapeTypeName}' is not compatible with shape data type '{DataType}'", shapetypename, this.Shape.DataType);
                return false;
            }

            for (int i = 0; i < Math.Min(parameters.Count(), fn.nParameters); i++)
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
using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Data container that describes a single model run — the root context
    /// name, the per-model instructions, global configuration, and the
    /// pending timed-change schedule.  All runtime logic lives in the
    /// concrete <see cref="cRuntimeModifierService"/> subclasses.
    /// </summary>
    // ------------------------------------------------------------------------
    public class cRuntimeModifier
    {
        #region Internal vars 

        public readonly string Root;
        public readonly IModelRunInstructions RunModel;
        public readonly cEwEConfiguration Configuration;
        public readonly Dictionary<int, cModificationsAtT> Changes;

        public const int FirstTimeStep = 1;
        public const int NoTimeStep = -1;

        #endregion // Internal vars 

        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="root">The root name of the runtime modifier, used to 
        /// determine if code modifications are allowed in the context of the 
        /// running model.</param>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The model-specific configuration and changes as 
        /// defined by the user.</param>
        // --------------------------------------------------------------------
        public cRuntimeModifier(string root, cEwEConfiguration config, IModelRunInstructions runmodel)
        {
            Root = root;
            Configuration = config;
            RunModel = runmodel;
            Changes = [];
        }
    }
}

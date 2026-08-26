using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    /// <summary>
    /// Data container for an Ecopath run.
    /// </summary>
    public class cEcopathModifier : cRuntimeModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="cEcopathModifier"/> class.
        /// </summary>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The Ecopath-specific configuration and changes as defined by the user.</param>
        public cEcopathModifier(cEwEConfiguration config, IModelRunInstructions runmodel)
            : base("ecopath", config, runmodel)
        {
        }
    }
}

using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    /// <summary>
    /// Data container for an Ecopath run.
    /// </summary>
    public class EcopathModifier : RuntimeModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EcopathModifier"/> class.
        /// </summary>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The Ecopath-specific configuration and changes as defined by the user.</param>
        public EcopathModifier(EwEConfiguration config, IModelRunInstructions runmodel)
            : base("ecopath", config, runmodel)
        {
        }

        /// <summary>
        /// Gets the strongly-typed run instructions for Ecopath.
        /// </summary>
        public EcopathRunInstructions MyRunModel => (EcopathRunInstructions)RunModel;
    }
}

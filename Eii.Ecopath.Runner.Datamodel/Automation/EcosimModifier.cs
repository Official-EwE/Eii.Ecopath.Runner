using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    /// <summary>
    /// Data container for an Ecosim run.
    /// </summary>
    public class EcosimModifier : RuntimeModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EcosimModifier"/> class.
        /// </summary>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The Ecosim-specific configuration and changes as defined by the user.</param>
        public EcosimModifier(EwEConfiguration config, EcosimRunInstructions runmodel)
            : base("ecosim", config, runmodel)
        {
        }

        /// <summary>
        /// Gets the strongly-typed run instructions for Ecosim.
        /// </summary>
        public EcosimRunInstructions MyRunModel => (EcosimRunInstructions)RunModel;
    }
}

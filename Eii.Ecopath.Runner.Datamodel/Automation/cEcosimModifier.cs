using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    /// <summary>
    /// Data container for an Ecosim run.
    /// </summary>
    public class cEcosimModifier : cRuntimeModifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="cEcosimModifier"/> class.
        /// </summary>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The Ecosim-specific configuration and changes as defined by the user.</param>
        public cEcosimModifier(cEwEConfiguration config, cEcosimRunInstructions runmodel)
            : base("ecosim", config, runmodel)
        {
        }

        /// <summary>
        /// Gets the strongly-typed run instructions for Ecosim.
        /// </summary>
        public cEcosimRunInstructions MyRunModel => (cEcosimRunInstructions)RunModel;
    }
}

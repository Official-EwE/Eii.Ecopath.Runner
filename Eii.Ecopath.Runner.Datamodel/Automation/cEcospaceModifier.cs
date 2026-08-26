using Eii.Ecopath.Runner.Datamodel.RunInstructions;

namespace Eii.Ecopath.Runner.Datamodel.Automation
{
    /// <summary>
    /// Data container for an Ecospace run.
    /// </summary>
    public class cEcospaceModifier : cRuntimeModifier
    {
        #region Construction / destruction

        /// <summary>
        /// Initializes a new instance of the <see cref="cEcospaceModifier"/> class.
        /// </summary>
        /// <param name="config">The EwE-wide configuration as defined by the user.</param>
        /// <param name="runmodel">The Ecospace-specific configuration and changes as defined by the user.</param>
        public cEcospaceModifier(cEwEConfiguration config, cEcospaceRunInstructions runmodel)
            : base("ecospace", config, runmodel)
        {
        }

        #endregion // Construction / destruction

        /// <summary>
        /// Gets the strongly-typed run instructions for Ecospace.
        /// </summary>
        public cEcospaceRunInstructions MyRunModel => (cEcospaceRunInstructions)RunModel;

        #region Internals 

        /// <summary>
        /// Determines whether the specified writer name is an ESRI ASCII (.asc) writer.
        /// </summary>
        /// <param name="name">The name of the writer to check.</param>
        /// <returns><c>true</c> if the writer is an ASC writer; otherwise, <c>false</c>.</returns>
        public bool IsASCWriter(string name)
        {
            return name.Contains("ecospaceasc");
        }

        /// <summary>
        /// Determines whether the specified writer name is a CSV writer.
        /// </summary>
        /// <param name="name">The name of the writer to check.</param>
        /// <returns><c>true</c> if the writer is a CSV writer; otherwise, <c>false</c>.</returns>
        public bool IsCSVWriter(string name)
        {
            return !IsASCWriter(name);
        }

        #endregion // Internals

    }
}

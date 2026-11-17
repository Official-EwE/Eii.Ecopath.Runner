using Eii.Ecopath.Runner.Datamodel.Utilities;
using System.Text.Json.Serialization;

namespace Eii.Ecopath.Runner.Datamodel.RunInstructions
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Container for receiving the various global EwE settings that the user 
    /// would want to configure.
    /// </summary>
    // ------------------------------------------------------------------------
    public class EwEConfiguration
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// The folder where all input resides in. Not serialized via JSON, but
        /// inferred from the location of the JSON file.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonIgnore]
        public string WorkFolder { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>
        /// The name of the model.eiixml file to load.
        /// </summary>
        // --------------------------------------------------------------------
        public string ModelFile { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>
        /// The file to load external spatial temporal data from.
        /// </summary>
        // --------------------------------------------------------------------
        public string ExtDataConfigFile { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>Flag that controls if saved output will be accompanied by 
        /// an contextual file header.
        /// </summary>
        // --------------------------------------------------------------------
        public bool SaveWithHeader { get; set; } = false;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index or the name of the Ecosim scenario to load.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStringOrNumberConverter))]
        public string EcosimScenario { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index or the name of the Ecosim time series dataset to load.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStringOrNumberConverter))]
        public string EcosimTimeseries { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index or the name of the Ecospace scenario to load.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStringOrNumberConverter))]
        public string EcospaceScenario { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index or the name of the Ecotracer scenario to load.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStringOrNumberConverter))]
        public string EcotracerScenario { get; set; } = string.Empty;

        // --------------------------------------------------------------------
        /// <summary>
        /// The number of years to run for.
        /// </summary>
        // --------------------------------------------------------------------
        public int RunYears { get; set; } = -1;
    }
}
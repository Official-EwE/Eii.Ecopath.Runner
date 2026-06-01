using System.Text.Json.Serialization;

namespace EwERunConsole.Instructions
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Container for receiving the various global EwE settings that the user 
    /// would want to configure.
    /// </summary>
    // ------------------------------------------------------------------------
    internal class cEwEConfiguration
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// The folder where all input resides in. Not serialized via JSON, but
        /// inferred from the location of the JSON file.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonIgnore]
        public string WorkFolder { get; set; } = "";

        // --------------------------------------------------------------------
        /// <summary>
        /// The name of the model.eiixml file to load.
        /// </summary>
        // --------------------------------------------------------------------
        public string ModelFile { get; set; } = "";

        // --------------------------------------------------------------------
        /// <summary>
        /// The file to load external spatial temporal data from.
        /// </summary>
        // --------------------------------------------------------------------
        public string ExtDataConfigFile { get; set; } = "";

        // --------------------------------------------------------------------
        /// <summary>Flag that controls if saved output will be accompanied by 
        /// an contextual file header.
        /// </summary>
        // --------------------------------------------------------------------
        public bool SaveWithHeader { get; set; } = false;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index of the Ecosim scenario to load.
        /// </summary>
        // --------------------------------------------------------------------
        public int EcosimScenario { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index of the Ecosim time series dataset to load.
        /// </summary>
        // --------------------------------------------------------------------
        public int EcosimTimeseries { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index of the Ecospace scenario to load.
        /// </summary>
        // --------------------------------------------------------------------
        public int EcospaceScenario { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// One-based index of the Ecotracer scenario to load.
        /// </summary>
        // --------------------------------------------------------------------
        public int EcotracerScenario { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// The number of years to run for.
        /// </summary>
        // --------------------------------------------------------------------
        public int RunYears { get; set; } = -1;
    }
}
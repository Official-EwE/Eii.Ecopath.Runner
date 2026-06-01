using EwERunConsole.Utilities;
using System.Text.Json.Serialization;

namespace EwERunConsole.Instructions
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Base container for receiving the various changes that the user would 
    /// like to make to the EwE models. This class is populated from the 
    /// JSON text file provided by the user.
    /// </summary>
    // ------------------------------------------------------------------------
    internal class cEwERunInstructions
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructah.
        /// </summary>
        // --------------------------------------------------------------------
        public cEwERunInstructions()
        {
            Configuration = new cEwEConfiguration();
            EcopathRun = new cEcopathRunInstructions();
            EcosimRun = new cEcosimRunInstructions();
            EcospaceRun = new cEcospaceRunInstructions();
            EcotracerRun = new cEcotracerRunInstructions();
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// The path to the JSON file that was deserialized.
        /// </summary>
        /// <remarks>
        /// For automation engine use, not part of the JSON deserialization set.
        /// </remarks>
        // --------------------------------------------------------------------
        [JsonIgnore]
        public string RunConfigFile = "";

        // --------------------------------------------------------------------
        /// <summary>
        /// The path to the output folder, which is actually obtained from the
        /// command line, not the JSON file.
        /// </summary>
        /// <remarks>
        /// For automation engine use, not part of the JSON deserialization set.
        /// </remarks>
        // --------------------------------------------------------------------
        [JsonIgnore]
        public string OutputFolder = "";

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the generic EwE configuration parameters.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<cEwEConfiguration>))]
        public cEwEConfiguration Configuration { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecopath configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<cEcopathRunInstructions>))]
        public cEcopathRunInstructions EcopathRun { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecosim configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<cEcosimRunInstructions>))]
        public cEcosimRunInstructions EcosimRun { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecospace configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<cEcospaceRunInstructions>))]
        public cEcospaceRunInstructions EcospaceRun { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecotracer configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<cEcotracerRunInstructions>))]
        public cEcotracerRunInstructions EcotracerRun { get; set; }
    }
}
using Eii.Ecopath.Runner.Datamodel.Utilities;
using System.Text.Json.Serialization;

namespace Eii.Ecopath.Runner.Datamodel.RunInstructions
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Base container for receiving the various changes that the user would 
    /// like to make to the EwE models. This class is populated from the 
    /// JSON text file provided by the user.
    /// </summary>
    // ------------------------------------------------------------------------
    public class EwERunInstructions
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructah.
        /// </summary>
        // --------------------------------------------------------------------
        public EwERunInstructions()
        {
            Configuration = new EwEConfiguration();
            EcopathRun = new EcopathRunInstructions();
            EcosimRun = new EcosimRunInstructions();
            EcospaceRun = new EcospaceRunInstructions();
            EcotracerRun = new EcotracerRunInstructions();
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
        [JsonConverter(typeof(JsonStrictConverter<EwEConfiguration>))]
        public EwEConfiguration Configuration { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecopath configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<EcopathRunInstructions>))]
        public EcopathRunInstructions EcopathRun { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecosim configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<EcosimRunInstructions>))]
        public EcosimRunInstructions EcosimRun { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecospace configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<EcospaceRunInstructions>))]
        public EcospaceRunInstructions EcospaceRun { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Get/set the Ecotracer configuration parameters and modifications.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(JsonStrictConverter<EcotracerRunInstructions>))]
        public EcotracerRunInstructions EcotracerRun { get; set; }
    }
}
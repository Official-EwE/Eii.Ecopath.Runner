using EwERunConsole.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace EwERunConsole.Instructions
{
    // --------------------------------------------------------------------
    /// <summary>
    /// Container class that represent a number of modifications for a 
    /// given EwE model at a given moment in time.
    /// </summary>
    // --------------------------------------------------------------------
    public class cModificationsAtT
    {
        public cModificationsAtT()
        {
            Date = "?";
            TimeStep = 0;
            modifications = new Dictionary<string, object>();
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// The date, in string format, that represent the <see cref="TimeStep"/>
        /// to make the modifications. Various date formats are supported.
        /// </summary>
        // --------------------------------------------------------------------
        public string Date { get; set; } = "";

        // --------------------------------------------------------------------
        /// <summary>
        /// The internal model one-based time step to make the modifications.
        /// </summary>
        /// <remarks>
        /// If left at 0, the timestep will be calculated from the provided
        /// <see cref="Date"/> in relation to the EwE model date and any time
        /// series that happen to be loaded.
        /// </remarks>
        // --------------------------------------------------------------------
        public int TimeStep { get; set; } = 0;

        // --------------------------------------------------------------------
        /// <summary>
        /// The actual changes.
        /// </summary>
        // --------------------------------------------------------------------
        [JsonConverter(typeof(DictionaryStringObjectConverter))]
        public Dictionary<string, object> modifications { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// Debug helper.
        /// </summary>
        // --------------------------------------------------------------------
        public override string ToString()
        {
            return string.Format("{0}: {1} change(s)", Date, modifications.Count());
        }
    }
}
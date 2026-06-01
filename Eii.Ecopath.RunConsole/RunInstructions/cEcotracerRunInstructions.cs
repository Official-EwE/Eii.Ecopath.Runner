using System.Collections.Generic;

namespace EwERunConsole.Instructions
{
    internal class cEcotracerRunInstructions : IModelRunInstructions
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Container for receiving the various changes that the user would 
        /// like to make to Ecotracer
        /// </summary>
        // --------------------------------------------------------------------
        public cEcotracerRunInstructions()
        {
            this.SaveContentCSV = new List<string>();
            this.Changes = new List<cModificationsAtT>();
        }

        public bool RunWithEcosim { get; set; } = false;
        public bool RunWithEcospace { get; set; } = false;

        // --------------------------------------------------------------------
        /// <summary>
        /// An array of string snippets that define the various potential
        /// CSV (.csv) formats that Ecosim can save while running.
        /// </summary>
        // --------------------------------------------------------------------
        public List<string> SaveContentCSV { get; set; }

        // --------------------------------------------------------------------
        /// <summary>
        /// The changes over time that the user wants to make to the running
        /// Ecosim model.
        /// </summary>
        // --------------------------------------------------------------------
        public List<cModificationsAtT> Changes { get; set; }
    }
}
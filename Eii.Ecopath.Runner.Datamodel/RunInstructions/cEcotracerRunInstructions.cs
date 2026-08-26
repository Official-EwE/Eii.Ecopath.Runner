using System.Collections.Generic;

namespace Eii.Ecopath.Runner.Datamodel.RunInstructions
{
    // --------------------------------------------------------------------
    /// <summary>
    /// Container for receiving the various changes that the user would 
    /// like to make to Ecotracer.
    /// </summary>
    // --------------------------------------------------------------------
    public class cEcotracerRunInstructions : IModelRunInstructions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="cEcotracerRunInstructions"/> class.
        /// </summary>
        public cEcotracerRunInstructions()
        {
            SaveContentCSV = new List<string>();
            Changes = new List<cModificationsAtT>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether Ecotracer should run with Ecosim.
        /// </summary>
        public bool RunWithEcosim { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether Ecotracer should run with Ecospace.
        /// </summary>
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
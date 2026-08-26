using System.Collections.Generic;

namespace Eii.Ecopath.Runner.Datamodel.RunInstructions
{
    // --------------------------------------------------------------------
    /// <summary>
    /// Container for receiving the various changes that the user would 
    /// like to make to Ecopath.
    /// </summary>
    // --------------------------------------------------------------------
    public class cEcopathRunInstructions : IModelRunInstructions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="cEcopathRunInstructions"/> class.
        /// </summary>
        public cEcopathRunInstructions()
        {
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// An array of string snippets that identify potentially numerous
        /// CSV formats that could be saved with Ecopath.
        /// </summary>
        // --------------------------------------------------------------------
        public List<string> SaveContentCSV { get; set; } = [];

        // --------------------------------------------------------------------
        /// <summary>
        /// The changes to make to Ecopath.
        /// </summary>
        // --------------------------------------------------------------------
        public List<cModificationsAtT> Changes { get; set; } = [];
    }
}
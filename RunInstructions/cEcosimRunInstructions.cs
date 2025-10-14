namespace EwERunConsole.Instructions
{
    internal class cEcosimRunInstructions : IModelRunInstructions
    {
        // --------------------------------------------------------------------
        /// <summary>
        /// Container for receiving the various changes that the user would 
        /// like to make to Ecosim.
        /// </summary>
        // --------------------------------------------------------------------
        public cEcosimRunInstructions()
        {
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// The first year to start saving Ecosim output for.
        /// </summary>
        // --------------------------------------------------------------------
        public int SaveFirstYear { get; set; } = 0;

        // --------------------------------------------------------------------
        /// <summary>
        /// Flag to save output annually (True) or monthly (False).
        /// </summary>
        // --------------------------------------------------------------------
        public bool SaveAnnual { get; set; } = true;

        // --------------------------------------------------------------------
        /// <summary>
        /// An array of string snippets that define the various potential
        /// CSV (.csv) formats that Ecosim can save while running.
        /// </summary>
        // --------------------------------------------------------------------
        public List<string> SaveContentCSV { get; set; } = [];

        // --------------------------------------------------------------------
        /// <summary>
        /// The changes over time that the user wants to make to the running
        /// Ecosim model.
        /// </summary>
        // --------------------------------------------------------------------
        public List<cModificationsAtT> Changes { get; set; } = [];
    }
}
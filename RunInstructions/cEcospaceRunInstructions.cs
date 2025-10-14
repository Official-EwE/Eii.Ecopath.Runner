namespace EwERunConsole.Instructions
{
    // --------------------------------------------------------------------
    /// <summary>
    /// Container for receiving the various changes that the user would 
    /// like to make to Ecospace.
    /// </summary>
    // --------------------------------------------------------------------
    internal class cEcospaceRunInstructions : IModelRunInstructions
    {
        public cEcospaceRunInstructions()
        {
        }

        // --------------------------------------------------------------------
        /// <summary>
        /// The number of Ecospace spin-up years to run.
        /// </summary>
        // --------------------------------------------------------------------
        public int RunSpinupYears { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// Flag, if true Ecospace will run in IBM mode. If false, Ecospace will
        /// run in the regular Multi-stanza mode. The Basic PDE option is not 
        /// supported in the run console.
        /// </summary>
        // --------------------------------------------------------------------
        public bool UseIBM { get; set; } = false;

        // --------------------------------------------------------------------
        /// <summary>
        /// The number of computational cores to allocate to Ecospace.
        /// </summary>
        /// <remarks>
        /// This flag should move to <see cref="cEwEConfiguration"/>
        /// </remarks>
        // --------------------------------------------------------------------
        public int MaxCores { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// The minimum capacity to use in the habitat capacity model when there
        /// is no capacity. Setting this to 0 will disable any habitat capacity
        /// corrections.
        /// </summary>
        // --------------------------------------------------------------------
        public float MinHabCap { get; set; } = -1;

        // --------------------------------------------------------------------
        /// <summary>
        /// The first year to start saving Ecospace output for.
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
        /// ESRI ASCII map (.asc) formats that Ecospace can save while running.
        /// </summary>
        // --------------------------------------------------------------------
        public List<string> SaveContentASC { get; set; } = [];

        // --------------------------------------------------------------------
        /// <summary>
        /// An array of string snippets that define the various potential
        /// CSV (.csv) formats that Ecospace can save while running.
        /// </summary>
        // --------------------------------------------------------------------
        public List<string> SaveContentCSV { get; set; } = [];

        // --------------------------------------------------------------------
        /// <summary>
        /// The changes over time that the user wants to make to the running
        /// Ecospace model.
        /// </summary>
        // --------------------------------------------------------------------
        public List<cModificationsAtT> Changes { get; set; } = [];
    }
}
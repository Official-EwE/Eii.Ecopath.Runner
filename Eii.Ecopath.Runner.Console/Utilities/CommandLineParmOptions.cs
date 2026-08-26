using CommandLine;

// ----------------------------------------------------------------------------
/// <summary>
/// Defines command-line argument options for EwERunConsole using CommandLineParser.
/// </summary>
// ----------------------------------------------------------------------------
class CommandLineParmOptions
{
    // ------------------------------------------------------------------------
    /// <summary>
    /// Gets or sets the path to the JSON run instructions file.
    /// </summary>
    // ------------------------------------------------------------------------
    [Option('i', "info", HelpText = "Run instructions file.", Required = false)]
    public string? RunInfo { get; set; }

    // ------------------------------------------------------------------------
    /// <summary>
    /// Gets or sets the output folder location for run results.
    /// </summary>
    // ------------------------------------------------------------------------
    [Option('o', "output", HelpText = "Output folder location")]
    public string? Output { get; set; }

    // ------------------------------------------------------------------------
    /// <summary>
    /// Gets or sets a value indicating whether to display the automation command tree.
    /// </summary>
    // ------------------------------------------------------------------------
    [Option('t', HelpText = "Show command tree")]
    public bool ShowTree { get; set; }

    // ------------------------------------------------------------------------
    /// <summary>
    /// Gets or sets a value indicating whether to display all commands being executed.
    /// </summary>
    [Option('c', HelpText = "Show all commands")]
    public bool ShowCommands { get; set; }

    // ------------------------------------------------------------------------
    /// <summary>
    /// Gets or sets a value indicating whether to generate automation documentation and exit.
    /// </summary>
    // ------------------------------------------------------------------------
    [Option('d', "docs", HelpText = "Write an automation command reference Markdown file and exit. Does not require --info.")]
    public bool Docs { get; set; }

}

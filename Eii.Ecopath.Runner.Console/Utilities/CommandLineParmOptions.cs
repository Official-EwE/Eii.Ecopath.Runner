using CommandLine;

class CommandLineParmOptions
{
    [Option('i', "info", HelpText = "Run instructions file.", Required = true)]
    public string? RunInfo { get; set; }

    [Option('o', "output", HelpText = "Output folder location")]
    public string? Output{ get; set; }

    [Option('t', HelpText = "Show command tree")]
    public bool ShowTree { get; set; } 

    [Option('c', HelpText = "Show all commands")]
    public bool ShowCommands { get; set; }

}

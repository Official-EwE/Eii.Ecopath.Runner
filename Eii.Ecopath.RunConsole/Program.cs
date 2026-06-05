using CommandLine;
using EwECore;
using EwERunConsole.Instructions;
using EwERunConsole.Runtime;
using EwEUtils.Logging;
using EwEUtils.Utilities;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

#pragma warning disable CS8604 // Possible null reference argument.

class Program
{
    public static int Main(string[] args)
    {
        string logFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EwERunConsole", "Logs");

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File($"{logFolder}\\log-.txt", outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {Message:lj}{NewLine}{Exception}", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        //Initialize LoggerFactory (to be used by the EwE Core and other components)
        LoggingContext.LoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
        });

        // Initialize logger after LoggerFactory is created
        var m_logger = LoggingContext.LoggerFactory.CreateLogger("EwERunConsole");

        ParserResult<CommandLineParmOptions> parms = Parser.Default.ParseArguments<CommandLineParmOptions>(args)
            .WithParsed(options => { ParseInstructions(options.RunInfo, options.Output, options.ShowTree, options.ShowCommands, m_logger); })
            .WithNotParsed(errors => { Complain(errors); });

        return 1;
    }

    /// <summary>
    /// Initialize the console app from the command line
    /// </summary>
    /// <param name="runinfofile"></param>
    /// <param name="outputfolder"></param>
    static void ParseInstructions(string runinfofile, string? outputfolder, bool showtree, bool showcommands, Microsoft.Extensions.Logging.ILogger logger)
    {
        cEwERunInstructions? info;

        // Need to do some restructuring. Might not need to RUN models, only load them to write out the command (tree).

        outputfolder = Path.Combine(outputfolder, Path.GetFileNameWithoutExtension(runinfofile));

        if (!cFileUtils.IsDirectoryAvailable(outputfolder, true))
        {
            Console.WriteLine("! Can't create output folder '{0}'", outputfolder);
            return;
        }

        // Start logging asap
        using (var cc = new cConsoleCopy(Path.Combine(outputfolder, "EwERunConsole_log.txt")))
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("EwERunConsole version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("EwE Core version {0}", cAssemblyUtils.GetVersion(cAssemblyUtils.GetAssemblyName(typeof(cCore))));
            Console.WriteLine("==========================================================================");
            Console.WriteLine();

            // Run info file accessible?
            if (!File.Exists(runinfofile))
            {
                Console.WriteLine("! Can't find run info file '{0}'", runinfofile);
                return;
            }

            try
            {
                // Read run info Json
                JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                string lines = File.ReadAllText(runinfofile);
                info = JsonSerializer.Deserialize<cEwERunInstructions>(lines, options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine("! {0}", ex.Message);
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine("! Can't parse run info file '{0}'. {1}", runinfofile, ex.Message);
                return;
            }

            // Has instructions?
            if (info == null)
            {
                Console.WriteLine("! Instructions missing from run info file '{0}'", runinfofile);
                return;
            }

            // Pass run info file name to the EwE engine
            info.RunConfigFile = Path.GetFullPath(runinfofile);
            // Pass output folder to the EwE engine
            info.OutputFolder = Path.GetFullPath(outputfolder);

            // Run the EwE engine
            cEwEEngine engine = new cEwEEngine(info);
            if (showtree | showcommands )
                engine.WriteAutomationCapabilities(showtree);
            else 
                if (engine.Run())
                    Console.WriteLine("Run completed");
                else
                    Console.WriteLine("! Run errors encountered");
        } // Using
    }

    static void Complain(IEnumerable<Error> errors)
    {
        // Bwaaaaaah
        foreach (var error in errors)
        {
            Console.WriteLine("! {0}", error.ToString());
        }
    }
}

#pragma warning restore CS8604 // Possible null reference argument.
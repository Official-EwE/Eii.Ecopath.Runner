using CommandLine;
using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using Eii.Ecopath.Runner.Datamodel.Utilities;
using Eii.Ecopath.Runner.Services.Runtime;
using EwECore;
using EwEUtils.Logging;
using EwEUtils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
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

        // Build a DI service provider so all services receive ILogger<T> via injection
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddSerilog(dispose: true));
        services.AddSingleton<ICoreService, cCoreService>();
        services.AddTransient<cNodeService>();
        services.AddTransient<cEcopathModifierService>();
        services.AddTransient<cEcosimModifierService>();
        services.AddTransient<cEcospaceModifierService>();
        services.AddTransient<cEwEEngine>();
        var sp = services.BuildServiceProvider();

        // Initialize LoggerFactory (to be used by EwE Core and other components that don't get ILogger via DI)
        LoggingContext.LoggerFactory = sp.GetRequiredService<ILoggerFactory>();

        // Initialize logger after LoggerFactory is created
        var m_logger = LoggingContext.LoggerFactory.CreateLogger("EwERunConsole");

        bool success = false;
        ParserResult<CommandLineParmOptions> parms = Parser.Default.ParseArguments<CommandLineParmOptions>(args)
            .WithParsed(options => { success = ParseInstructions(options.RunInfo, options.Output, options.ShowTree, options.ShowCommands, options.Docs, m_logger, sp); })
            .WithNotParsed(errors => { Complain(errors); });

        return success ? 1 : 0;
    }

    /// <summary>
    /// Initialize the console app from the command line
    /// </summary>
    /// <param name="runinfofile"></param>
    /// <param name="outputfolder"></param>
    static bool ParseInstructions(string? runinfofile, string? outputfolder, bool showtree, bool showcommands, bool generateDocs, Microsoft.Extensions.Logging.ILogger logger, IServiceProvider sp)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        cEwERunInstructions? info;

        // Need to do some restructuring. Might not need to RUN models, only load them to write out the command (tree).

        if (generateDocs)
        {
            string docsFolder = "~Documentation";
            string docsPath = Path.Combine(docsFolder, "automation-commands.md");
            File.WriteAllText(docsPath, Eii.Ecopath.Runner.Services.Automation.cAutomationDocumentation.GenerateMarkdown());
            logger.LogInformation("Automation command reference written to '{Path}'", docsPath);
            Console.WriteLine("Automation command reference written to '{0}'", docsPath);
            return true;
        }

        if (string.IsNullOrEmpty(runinfofile))
        {
            logger.LogError("No run-info file specified. Use --info or --docs.");
            Console.WriteLine("! No run-info file specified. Use --info or --docs.");
            return false;
        }

        outputfolder = Path.Combine(outputfolder, Path.GetFileNameWithoutExtension(runinfofile));

        if (!cFileUtils.IsDirectoryAvailable(outputfolder, true))
        {
            Console.WriteLine("! Can't create output folder '{0}'", outputfolder);
            return false;
        }

        // Start logging asap
        using (var cc = new cConsoleCopy(Path.Combine(outputfolder, "EwERunConsole_log.txt")))
        {
            Console.WriteLine("==========================================================================");
            Console.WriteLine("EwERunConsole version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("EwE Core version {0}", cAssemblyUtils.GetVersion(cAssemblyUtils.GetAssemblyName(typeof(cCore))));
            Console.WriteLine("Executed on {0}", DateTime.Now);
            Console.WriteLine("==========================================================================");
            Console.WriteLine();

            // Run info file accessible?
            if (!File.Exists(runinfofile))
            {
                Console.WriteLine("! Can't find run info file '{0}'", runinfofile);
                return false;
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
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("! Can't parse run info file '{0}'. {1}", runinfofile, ex.Message);
                return false;
            }

            // Has instructions?
            if (info == null)
            {
                Console.WriteLine("! Instructions missing from run info file '{0}'", runinfofile);
                return false;
            }

            // Pass run info file name to the EwE engine
            info.RunConfigFile = Path.GetFullPath(runinfofile);
            // Pass output folder to the EwE engine
            info.OutputFolder = Path.GetFullPath(outputfolder);

            // Run the EwE engine
            cEwEEngine engine = sp.GetRequiredService<cEwEEngine>();
            if (showtree | showcommands)
                engine.WriteAutomationCapabilities(info, showtree);
            else if (engine.Run(info) == false)
            {
                Console.WriteLine("! Run errors encountered");
                return false;
            }
        } // Using
        stopwatch.Stop();
        Console.WriteLine("Run completed in {0}", stopwatch.Elapsed);
        return true;
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
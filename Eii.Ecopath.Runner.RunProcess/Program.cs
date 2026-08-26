using Eii.BlobStore;
using Eii.BlobStore.S3;
using Eii.Ecopath.Runner.Services.Runtime;
using EwEUtils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EwERunProcess
{
    /// <summary>
    /// Entry point for EwERunProcess - a background service that executes EwE model runs with blob storage support.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Application entry point that configures services, sets up logging, and executes the EwE run process.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        public static async Task Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var inputDirectory = Environment.GetEnvironmentVariable("INPUT_DIRECTORY");
            var outputDirectory = Environment.GetEnvironmentVariable("OUTPUT_DIRECTORY");

            if (string.IsNullOrEmpty(inputDirectory))
                throw new InvalidOperationException("Environment variable INPUT_DIRECTORY is not set.");
            if (string.IsNullOrEmpty(outputDirectory))
                throw new InvalidOperationException("Environment variable OUTPUT_DIRECTORY is not set.");

            Directory.CreateDirectory(inputDirectory);
            Directory.CreateDirectory(outputDirectory);

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddTransient<EwERunProcessService>();
                    services.AddSingleton<ICoreService, cCoreService>();
                    services.AddTransient<cNodeService>();
                    services.AddTransient<cEcopathModifierService>();
                    services.AddTransient<cEcosimModifierService>();
                    services.AddTransient<cEcospaceModifierService>();
                    services.AddTransient<cEwEEngine>();
                    services.AddSingleton<IBlobStore>(sp =>
                    {
                        var blobLogger = sp.GetRequiredService<ILogger<Program>>();

                        // if AWS_ACCESS_KEY_ID is set, use S3BlobStore. 
                        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")))
                        {
                            blobLogger.LogInformation("Environment variable AWS_ACCESS_KEY_ID found. Using S3BlobStore");
                            return new S3BlobStore(
                                Environment.GetEnvironmentVariable("AWS_S3_ENDPOINT"),
                                Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                                Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
                                Environment.GetEnvironmentVariable("AWS_BUCKET_NAME"),
                                inputBasePrefix: $"ewerunprocess/{inputDirectory}", outputBasePrefix: $"ewerunprocess/{outputDirectory}",
                                localInputRoot: inputDirectory, localOutputRoot: outputDirectory,
                                Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN"));
                        }

                        // Default local Filesystem
                        blobLogger.LogInformation("Using LocalBlobStore");
                        return new LocalBlobStore(inputRoot: inputDirectory, outputRoot: outputDirectory);
                    });
                })
                .Build();

            // Initialize LoggerFactory of EwE sources that don't get the ILogger via Dependency injection
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            LoggingContext.LoggerFactory = LoggerFactory.Create(logBuilder =>
            {
                logBuilder.AddConfiguration(configuration.GetSection("Logging"));   // so you can add a 'Logging' section to the appsettings.json to configure logging
                logBuilder.AddConsole(options =>
                {
                    options.FormatterName = "systemd";
                });
                logBuilder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
                logBuilder.AddDebug();
            });

            // Create a logger for the Program class
            var logger = LoggingContext.LoggerFactory.CreateLogger<Program>();
            logger.LogInformation("EwERunProcess starting up.......................");

            logger.LogInformation("============================= Logging All Environment Variables =============================");
            foreach (System.Collections.DictionaryEntry envVar in Environment.GetEnvironmentVariables())
            {
                logger.LogInformation("{Key}: {Value}", envVar.Key, envVar.Value);
            }
            logger.LogInformation("============================= End of Environment Variables =============================");

            logger.LogInformation("EwERunProcess running.......................");

            var service = host.Services.GetRequiredService<EwERunProcessService>();
            var result = await service.Run(inputDirectory, outputDirectory);

            stopwatch.Stop();
            logger.LogInformation("EwERunProcess took {Duration} and is shutting down.......................", stopwatch.Elapsed);
        }
    }
}
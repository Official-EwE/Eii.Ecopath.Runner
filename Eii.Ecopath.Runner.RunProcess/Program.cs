using Eii.BlobStore;
using Eii.BlobStore.S3;
using EwEUtils.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EwERunProcess
{
    internal class Program
    {
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
                    services.AddSingleton<IBlobStore>(sp =>
                    {
                        var blobLogger = sp.GetRequiredService<ILogger<Program>>();

                        // if AWS_ACCESS_KEY_ID is set, use S3BlobStore. The AWS env vars are read in LoadVaultSecretsInEnvironmentVariables
                        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")))
                        {
                            blobLogger.LogInformation("Environment variable AWS_ACCESS_KEY_ID found. Using S3BlobStore");
                            return new S3BlobStore(
                                Environment.GetEnvironmentVariable("AWS_S3_ENDPOINT"),
                                Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                                Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY"),
                                Environment.GetEnvironmentVariable("AWS_BUCKET_NAME"),
                                inputBasePrefix: $"ewerunprocess/{inputDirectory}", outputBasePrefix: $"ewerunprocess/{outputDirectory}",
                                localInputRoot: inputDirectory, localOutputRoot: outputDirectory);
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

            LoadVaultSecretsInEnvironmentVariables(configuration);
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

        /// <summary>
        /// Loads secrets from Vault into environment variables. Expects the following environment variables to be set to connect to Vault and locate the secrets:
        /// VAULT_ADDR, VAULT_TOKEN, VAULT_TOP_DIR, VAULT_RELATIVE_PATH, VAULT_MOUNT
        /// </summary>
        public static void LoadVaultSecretsInEnvironmentVariables(IConfiguration configuration)
        {
            var vaultAddr = Environment.GetEnvironmentVariable("VAULT_ADDR");
            var vaultToken = Environment.GetEnvironmentVariable("VAULT_TOKEN") ?? configuration["VAULT_TOKEN"];     // VAULT_TOKEN must be stored as a secret, so it isn't added to Git
            var vaultTopDir = Environment.GetEnvironmentVariable("VAULT_TOP_DIR");
            var vaultRelativePath = Environment.GetEnvironmentVariable("VAULT_RELATIVE_PATH");
            var vaultMount = Environment.GetEnvironmentVariable("VAULT_MOUNT");
            if (string.IsNullOrEmpty(vaultAddr) || string.IsNullOrEmpty(vaultToken) || string.IsNullOrEmpty(vaultTopDir) || string.IsNullOrEmpty(vaultRelativePath) || string.IsNullOrEmpty(vaultMount))
            {
                Console.WriteLine("Vault Addr, Token, Top Dir, Relative Path, or Mount not set in environment variables. Skipping Vault loading.");
                return;
            }
            var vaultClient = new VaultSharp.VaultClient(new VaultSharp.VaultClientSettings(vaultAddr, new VaultSharp.V1.AuthMethods.Token.TokenAuthMethodInfo(vaultToken)));
            // Assuming secrets are stored under "secret/data/surimi"
            var secretPath = $"{vaultTopDir}/{vaultRelativePath}";
            try
            {
                var secret = vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(secretPath, mountPoint: vaultMount).ConfigureAwait(false).GetAwaiter().GetResult();
                foreach (var kv in secret.Data.Data)
                {
                    Environment.SetEnvironmentVariable(kv.Key, kv.Value.ToString());
                    Console.WriteLine($"Loaded secret '{kv.Key}' from Vault into environment variables.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading secrets from Vault: {ex.Message}");
            }
        }
    }
}
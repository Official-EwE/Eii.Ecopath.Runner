using Eii.BlobStore;
using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using Eii.Ecopath.Runner.Services.Runtime;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EwERunProcess
{
    internal class EwERunProcessService
    {
        private readonly ILogger<EwERunProcessService> m_logger;
        private readonly IBlobStore _blobStore;

        public EwERunProcessService(ILogger<EwERunProcessService> logger, IBlobStore blobStore)
        {
            m_logger = logger;
            _blobStore = blobStore;
        }

        internal async Task<int> Run(string inputDirectory, string outputDirectory)
        {
            m_logger.LogInformation("Start running...");

            var runInfoPath = Environment.GetEnvironmentVariable("RUN_INFO_PATH");

            if (string.IsNullOrEmpty(runInfoPath))
                throw new InvalidOperationException("Environment variable RUN_INFO_PATH is not set.");

            //var instructions = await GetInstructions(args[0]);

            cEwERunInstructions? info;


            // Run info file accessible?
            if (!File.Exists(runInfoPath))
            {
                Console.WriteLine("! Can't find run info file '{0}'", runInfoPath);
                throw new InvalidOperationException($"Run info file '{runInfoPath}' cannot be found");
            }

            try
            {
                // Read run info Json
                JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                string lines = File.ReadAllText(runInfoPath);
                info = JsonSerializer.Deserialize<cEwERunInstructions>(lines, options);
            }
            catch (JsonException ex)
            {
                Console.WriteLine("! {0}", ex.Message);
                throw new InvalidOperationException($"Error parsing run info file '{runInfoPath}': {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("! Can't parse run info file '{0}'. {1}", runInfoPath, ex.Message);
                throw new InvalidOperationException($"Error parsing run info file '{runInfoPath}': {ex.Message}", ex);
            }

            // Has instructions?
            if (info == null)
            {
                Console.WriteLine("! Instructions missing from run info file '{0}'", runInfoPath);
                throw new InvalidOperationException($"Instructions missing from run info file '{runInfoPath}'");
            }

            // check if model file exists in blob store
            if (!await _blobStore.ExistsAsync(info.Configuration.ModelFile, PathType.Input))
                throw new FileNotFoundException($"EwE model file '{info.Configuration.ModelFile}' cannot be found");

            // If connected to a remote blob store, copy the remote directory locally to the input folder
            var localModelFiles = await _blobStore.CopyToLocalDirectoryOrIgnoreAsync("", PathType.Input);

            string baseDir = AppContext.BaseDirectory;

            // You only have to pass the RunConfigFile so it can determine the directory of that file. This used to be the "AnchovyBay_runinfo.json", but in the API that is replaced by the posted instructions object. 
            // This should be changed in the EwERunConsole project. You should be able to pass the input folder.
            info.RunConfigFile = Path.Combine(baseDir, inputDirectory, info.Configuration.ModelFile);
            // Pass output folder to the EwE engine
            info.OutputFolder = Path.GetFullPath(outputDirectory);

            // Run the EwE engine
            cEwEEngine engine = new cEwEEngine(info);
            if (engine.Run())
                Console.WriteLine("Run completed");
            else
                Console.WriteLine("! Run errors encountered");

            return 1;
        }
    }
}


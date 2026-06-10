using Eii.BlobStore;
using Eii.Ecopath.Runner.Datamodel.RunInstructions;
using Eii.Ecopath.Runner.Datamodel.Utilities;
using Eii.Ecopath.Runner.Services.Runtime;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EwERunProcess
{
    internal class EwERunProcessService
    {
        private readonly ILogger<EwERunProcessService> m_logger;
        private readonly IBlobStore _blobStore;
        cEwERunInstructions? runInstructions;

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

            // Run info file accessible?
            if (!File.Exists(runInfoPath))
            {
                Console.WriteLine("! Can't find run info file '{0}'", runInfoPath);
                throw new InvalidOperationException($"Run info file '{runInfoPath}' cannot be found");
            }

            // Copy run info file to input directory
            var runInfoDestPath = Path.Combine(inputDirectory, Path.GetFileName(runInfoPath));
            File.Copy(runInfoPath, runInfoDestPath, overwrite: true);
            m_logger.LogInformation("Copied run info file to '{0}'", runInfoDestPath);

            try
            {
                // Read run info Json
                JsonSerializerOptions options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                string lines = File.ReadAllText(runInfoPath);
                runInstructions = JsonSerializer.Deserialize<cEwERunInstructions>(lines, options);
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
            if (runInstructions == null)
            {
                Console.WriteLine("! Instructions missing from run info file '{0}'", runInfoPath);
                throw new InvalidOperationException($"Instructions missing from run info file '{runInfoPath}'");
            }

            // check if model file exists in blob store
            if (!await _blobStore.ExistsAsync(runInstructions.Configuration.ModelFile, PathType.Input))
                throw new FileNotFoundException($"EwE model file '{runInstructions.Configuration.ModelFile}' cannot be found");

            // If connected to a remote blob store, copy the remote directory locally to the input folder
            var localModelFiles = await _blobStore.CopyToLocalDirectoryOrIgnoreAsync("", PathType.Input);

            // You only have to pass the RunConfigFile so it can determine the directory of that file. This used to be the "AnchovyBay_runinfo.json", but in the RunProcess that is replaced by the database. 
            // This should be changed in the EwERunConsole project. You should be able to pass the input folder.
            runInstructions.RunConfigFile = Path.Combine(AppContext.BaseDirectory, inputDirectory, runInstructions.Configuration.ModelFile);
            // Pass output folder to the EwE engine
            runInstructions.OutputFolder = Path.GetFullPath(outputDirectory);

            // Redirect console output to a log file in the output directory, so it can be uploaded to the blob store after the run. The log file is released after the using block, so it can be uploaded.
            using (new cConsoleCopy(Path.Combine(outputDirectory, "EwERunProcess_log.txt")))
            {
                // Run the EwE engine
                cEwEEngine engine = new cEwEEngine(runInstructions);
                if (engine.Run())
                    Console.WriteLine("Run completed");
                else
                    Console.WriteLine("! Run errors encountered");
            } // cConsoleCopy disposed here — log file released before UploadDirectoryOrIgnoreAsync


            var outputFiles = await _blobStore.UploadDirectoryOrIgnoreAsync("", PathType.Output);
            Console.WriteLine("Nr of output files: {0}", outputFiles.Count());

            return 1;
        }
    }
}


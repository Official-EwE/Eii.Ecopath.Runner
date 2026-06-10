using Minio;
using Minio.DataModel.Args;
using System.Collections;
using System.Diagnostics;
using System.Text;

internal class Program
{
    Logger logger = new Logger();

    public static async Task Main(string[] args)
    {
        var program = new Program();
        program.logger.Log("Hello, World! It's me!");

        //await program.PrintFileNamesFromMinIO();

        Console.WriteLine("Waiting for 5 seconds...");
        //await Task.Delay(5000);

        program.logger.Log("Environment variables:");
        IDictionary environmentVariables = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry entry in environmentVariables)
        {
            program.logger.Log($"{entry.Key}: {entry.Value}");
        }

        program.logger.Log($"Current Directory: {Directory.GetCurrentDirectory()}");
        string filepath = $"{program.logger.FileName}";

        program.logger.Log($"Try to write Logfile to {filepath}");

        program.logger.WriteLogFile(filepath);
        await program.CopyRunInfoFile();

        Console.WriteLine("Waiting for 20 seconds...");
        await Task.Delay(20000);
        Console.WriteLine("Bye bye!!!! Program ends..");
    }

    async Task CopyRunInfoFile()
    {
        var runInfoPath = Environment.GetEnvironmentVariable("RUN_INFO_PATH");
        if (string.IsNullOrEmpty(runInfoPath))
        {
            logger.Log("RUN_INFO_PATH environment variable is not set, skipping copy.");
            return;
        }

        var destFileName = Path.GetFileName(runInfoPath);
//        var destPath = $"output-data/{destFileName}";
        var destPath = $"/output-data/{destFileName}";                        // This must be changed before push to container!!!!
        logger.Log($"Copying '{runInfoPath}' to '{destPath}'");
        try
        {
            File.Copy(runInfoPath, destPath, overwrite: true);
            logger.Log($"Successfully copied {destFileName} to /output-data/");
        }
        catch (Exception ex)
        {
            logger.Log($"Failed to copy runinfo.json: {ex.Message}");
        }

        logger.Log($"Copying #2 '{runInfoPath}' to 'ewerunprocess/{destFileName}'");
        await WriteFileToS3(runInfoPath, $@"ewerunprocess/{destFileName}");

    }

    /// <summary>
    /// Write a file to S3 (MinIO) using the MinIO .NET SDK. The S3 credentials and endpoint are read from environment variables.
    /// </summary>
    /// <param name="srcFilePath">The path of the source file to upload. Something like /etc/config/runinfo.json</param>
    /// <param name="destFilePath">The destination file path in the S3 bucket. Something like ewerunprocess/runinfo.json. The ewerunprocess folder will be created if it doesn't exist.</param>
    /// <returns></returns>
    async Task WriteFileToS3(string srcFilePath, string destFilePath)
    {
        var minio = GetClient();

        var bucketName = "oidc-rikkert";
        //var location = "us-east-1";
        var objectName = destFilePath;
        var filePath = srcFilePath;
        var contentType = "text/plain";

        // Upload a file to bucket.
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithFileName(filePath)
            .WithContentType(contentType);
        var res = await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        Console.WriteLine("Successfully uploaded " + objectName + ". Res= " + res.ResponseStatusCode);
    }

    IMinioClient GetClient()
    {
        var endpoint = Environment.GetEnvironmentVariable("AWS_S3_ENDPOINT");    // "minio.dive.edito.eu";
        var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");  // "UVFQ060EITDQ25CDY0FQ";
        var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");  // "v39+pt8QrUabvtQ8gsWF3u5vTXWW5MbVvexVSkDh";
        var sessionToken = Environment.GetEnvironmentVariable("AWS_SESSION_TOKEN");  // "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJhY2Nlc3NLZXkiOiJVVkZRMDYwRUlURFEyNUNEWTBGUSIsImFjciI6IjAiLCJhbGxvd2VkLW9yaWdpbnMiOlsiKiJdLCJhdWQiOlsibWluaW8iLCJhY2NvdW50Il0sImF1dGhfdGltZSI6MTc0MDM4MjA2NywiYXpwIjoib255eGlhLW1pbmlvIiwiZW1haWwiOiJyaWsua3JlZWZ0ZW5iZXJnQHNwaW5zb2Z0Lm5sIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImV4cCI6MTc0MDQ4MDcyNCwiZmFtaWx5X25hbWUiOiJLcmVlZnRlbmJlcmciLCJnaXZlbl9uYW1lIjoiUmlrIiwiZ3JvdXBzIjpbIkVESVRPX1VTRVIiXSwiaWF0IjoxNzQwMzk0MzI0LCJpc3MiOiJodHRwczovL2F1dGguZGl2ZS5lZGl0by5ldS9hdXRoL3JlYWxtcy9kYXRhbGFiIiwianRpIjoiZTM0OWY1NTktMGNmMC00Yjk0LWI3YzctN2I4OGMxMjRhYWMwIiwibmFtZSI6IlJpayBLcmVlZnRlbmJlcmciLCJwb2xpY3kiOiJzdHNvbmx5IiwicHJlZmVycmVkX3VzZXJuYW1lIjoicmlra2VydCIsInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJkZWZhdWx0LXJvbGVzLWRhdGFsYWIiLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19LCJtaW5pbyI6eyJyb2xlcyI6WyJzdHNvbmx5Il19fSwic2NvcGUiOiJvcGVuaWQgZW1haWwgcHJvZmlsZSIsInNlc3Npb25fc3RhdGUiOiJiN2E1YzJlMS04ODdlLTRiMzUtOGU1MC0xZmM2NDU3ODFiMzciLCJzaWQiOiJiN2E1YzJlMS04ODdlLTRiMzUtOGU1MC0xZmM2NDU3ODFiMzciLCJzdWIiOiIzMTJmZDE4MC1jYzVjLTQ1YmQtYTY4OS1hNDBjMDI4NmJjYjQiLCJ0eXAiOiJCZWFyZXIifQ.CQIaPIjKP3M8JHHICVJacSEuzW6vAgNm556ZTNrkCINBWoeXNVV2Yy2MlIndt4xZHjjSU4bQ_a0b7CFLtDbP1w";

        if (string.IsNullOrEmpty(endpoint))
            throw new InvalidOperationException("Environment variable AWS_S3_ENDPOINT is not set.");
        if (string.IsNullOrEmpty(accessKey))
            throw new InvalidOperationException("Environment variable AWS_ACCESS_KEY_ID is not set.");
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("Environment variable AWS_SECRET_ACCESS_KEY is not set.");
        if (string.IsNullOrEmpty(sessionToken))
            throw new InvalidOperationException("Environment variable AWS_SESSION_TOKEN is not set.");

        // Initialize the client with access credentials.
        return new MinioClient()
                        .WithEndpoint(endpoint, 443)
                        .WithCredentials(accessKey, secretKey)
                        .WithSessionToken(sessionToken)
                        .WithSSL(true)
                        .Build();
    }

    async Task PrintFileNamesFromMinIO()
    {
        var minio = GetClient();

        // Create an async task for listing buckets.
        var getListBucketsTask = await minio.ListBucketsAsync().ConfigureAwait(false);

        // Iterate over the list of buckets.
        foreach (var bucket in getListBucketsTask.Buckets)
        {
            logger.Log(bucket.Name + " " + bucket.CreationDateDateTime);
            ListObjectsArgs args = new ListObjectsArgs()
                                      .WithBucket(bucket.Name)
                                      //                                      .WithPrefix("prefix")
                                      .WithRecursive(true);

            await foreach (var item in minio.ListObjectsEnumAsync(args).ConfigureAwait(false))
            {
                logger.Log(item.Key);
            }
        }

        logger.Log("Environment variables:");
        IDictionary environmentVariables = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry entry in environmentVariables)
        {
            logger.Log($"{entry.Key}: {entry.Value}");
        }
    }
}

internal class Logger
{
    StringBuilder log = new StringBuilder();

    internal string FileName { get; set; } = $"LogFile-{DateTime.Now.ToString("MM-dd-yyy-HHmmss")}.txt";
    internal void Log(string msg)
    {
        Console.WriteLine(msg);
        this.log.AppendLine(msg);
    }
    internal void WriteLogFile(string filepath)
    {


        Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"../")));
        Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");


        try
        {
            System.IO.File.AppendAllText($"/output-data/{filepath}", log.ToString());
            Console.WriteLine($"File written to /output-data/{filepath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write file: {ex.Message}");
        }
    }
}

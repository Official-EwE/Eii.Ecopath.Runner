using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Eii.Ecopath.Runner.Console.Tests
{
    /// <summary>
    /// Result of a single EwERunConsole process invocation.
    /// </summary>
    internal record cConsoleRunResult(
        int ExitCode,
        string StdOut,
        string StdErr,
        string ActualOutputFolder);

    /// <summary>
    /// Launches EwERunConsole.exe as a child process and captures its output.
    /// </summary>
    internal static class cConsoleRunner
    {
        /// <summary>
        /// Full path to EwERunConsole.exe, expected next to the test assembly.
        /// </summary>
        internal static readonly string ExePath =
            Path.Combine(AppContext.BaseDirectory, "EwERunConsole.exe");

        // Default timeout for any single run.
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Runs EwERunConsole with a run-info file and a unique output id,
        /// using the test assembly directory as the working directory.
        /// </summary>
        /// <param name="runInfoRelPath">
        ///   Path to the run-info JSON file, relative to
        ///   <see cref="AppContext.BaseDirectory"/>.
        /// </param>
        /// <param name="uniqueId">
        ///   A unique identifier (typically a GUID) used to create a
        ///   collision-free output subfolder under <c>Testoutput\</c>.
        /// </param>
        /// <param name="ct">Optional cancellation token.</param>
        internal static async Task<cConsoleRunResult> RunAsync(
            string runInfoRelPath,
            string uniqueId,
            CancellationToken ct = default)
        {
            string runInfoPath = Path.Combine(AppContext.BaseDirectory, runInfoRelPath);
            string outputBase = Path.Combine("Testoutput", uniqueId);

            // ArgumentList handles quoting automatically — do not wrap paths in extra quotes.
            string[] args = ["-i", runInfoPath, "-o", outputBase];

            string actualOutputFolder = Path.Combine(
                AppContext.BaseDirectory,
                "Testoutput",
                uniqueId,
                Path.GetFileNameWithoutExtension(runInfoRelPath));

            return await RunCoreAsync(args, actualOutputFolder, ct);
        }

        /// <summary>
        /// Runs EwERunConsole with a completely arbitrary argument list.
        /// Use this overload for negative tests (e.g. empty args).
        /// </summary>
        internal static async Task<cConsoleRunResult> RunAsync(
            string[] rawArgs,
            CancellationToken ct = default)
        {
            return await RunCoreAsync(rawArgs, actualOutputFolder: string.Empty, ct);
        }

        private static async Task<cConsoleRunResult> RunCoreAsync(
            IEnumerable<string> args,
            string actualOutputFolder,
            CancellationToken ct)
        {
            using var timeoutCts = new CancellationTokenSource(DefaultTimeout);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            var psi = new ProcessStartInfo(ExePath)
            {
                WorkingDirectory = AppContext.BaseDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            foreach (string arg in args)
                psi.ArgumentList.Add(arg);

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

            var stdOutDone = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var stdErrDone = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data is null)
                    stdOutDone.TrySetResult(true);
                else
                    stdOut.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data is null)
                    stdErrDone.TrySetResult(true);
                else
                    stdErr.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(linkedCts.Token);
            await Task.WhenAll(stdOutDone.Task, stdErrDone.Task);

            return new cConsoleRunResult(
                process.ExitCode,
                stdOut.ToString(),
                stdErr.ToString(),
                actualOutputFolder);
        }
    }
}

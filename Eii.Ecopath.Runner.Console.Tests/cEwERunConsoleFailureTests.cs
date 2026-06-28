using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Eii.Ecopath.Runner.Console.Tests
{
    /// <summary>
    /// Integration tests that verify EwERunConsole exits with code 0
    /// and reports the appropriate error message for invalid inputs.
    /// </summary>
    public class cEwERunConsoleFailureTests
    {
        [Fact]
        public async Task NonExistentRunInfoFile_Fails()
        {
            // Arrange
            string id = Guid.NewGuid().ToString("N");

            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\DoesNotExist_runinfo.json", id);

            // Assert
            result.ExitCode.Should().Be(0, because: result.StdOut);
            result.StdOut.Should().Contain("! Can't find run info file");
        }

        [Fact]
        public async Task InvalidJson_Fails()
        {
            // Arrange
            string id = Guid.NewGuid().ToString("N");

            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_InvalidJson.json", id);

            // Assert
            result.ExitCode.Should().Be(0, because: result.StdOut);
            result.StdOut.Should().Contain("!");
        }

        [Fact]
        public async Task NoArguments_Fails()
        {
            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync([]);

            // Assert
            result.ExitCode.Should().Be(0);
        }
    }
}

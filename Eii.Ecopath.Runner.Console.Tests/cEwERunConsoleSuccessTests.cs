using FluentAssertions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Eii.Ecopath.Runner.Console.Tests
{
    /// <summary>
    /// Integration tests that verify EwERunConsole exits with code 1
    /// and produces the expected output for valid run configurations.
    /// </summary>
    public class cEwERunConsoleSuccessTests
    {
        [Fact]
        public async Task AnchovyBay_EcosimAndEcospace_Succeeds()
        {
            // Arrange
            string id = Guid.NewGuid().ToString("N");

            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_runinfo.json", id);

            // Assert
            result.ExitCode.Should().Be(1, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");
            File.Exists(Path.Combine(result.ActualOutputFolder, "EwERunConsole_log.txt"))
                .Should().BeTrue("the console log file should be written to the output folder");
        }

        [Fact]
        public async Task AnchovyBay_EcosimOnly_Succeeds()
        {
            // Arrange
            string id = Guid.NewGuid().ToString("N");

            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_EcosimOnly_runinfo.json", id);

            // Assert
            result.ExitCode.Should().Be(1, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");
            Directory.GetFiles(result.ActualOutputFolder, "*.csv")
                .Should().NotBeEmpty("Ecosim should write at least one CSV output file");
        }

        [Fact]
        public async Task AnchovyBay_EcopathOnly_Succeeds()
        {
            // Arrange
            string id = Guid.NewGuid().ToString("N");

            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_EcopathOnly_runinfo.json", id);

            // Assert
            result.ExitCode.Should().Be(1, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");
        }

        [Fact]
        [Trait("Category", "Slow")]
        public async Task VLIZ_EcosimOnly_Succeeds()
        {
            // Arrange
            string id = Guid.NewGuid().ToString("N");

            // Act
            cConsoleRunResult result = await cConsoleRunner.RunAsync(
                @"Testdata\VLIZ\VLIZ_runinfo.json", id);

            // Assert
            result.ExitCode.Should().Be(1, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");
            Directory.GetFiles(result.ActualOutputFolder, "*.csv")
                .Should().NotBeEmpty("Ecosim should write at least one CSV output file");
        }
    }
}

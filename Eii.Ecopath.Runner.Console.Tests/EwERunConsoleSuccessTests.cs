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
    public class EwERunConsoleSuccessTests
    {
        [Fact]
        public async Task AnchovyBay_EcopathOnly_Succeeds()
        {
            // Arrange
            string outputFolder = Path.Combine(
                AppContext.BaseDirectory, "Testoutput", "AnchovyBay", "AnchovyBay_EcopathOnly_runinfo");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, recursive: true);

            // Act
            ConsoleRunResult result = await ConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_EcopathOnly_runinfo.json", "AnchovyBay");

            // Assert
            result.ExitCode.Should().Be(0, because: $"{result.StdOut}{Environment.NewLine}{result.StdErr}");
            result.StdOut.Should().Contain("Run completed");
            result.ActualOutputFolder.Should().Be(outputFolder);
            Directory.Exists(outputFolder).Should().BeTrue("the run should create its output folder");
            Directory.GetFiles(outputFolder).Should().BeEquivalentTo(
                Path.Combine(outputFolder, "basic_estimates.csv"),
                Path.Combine(outputFolder, "ecopath_stats.csv"),
                Path.Combine(outputFolder, "EwERunConsole_log.txt"));
        }

        [Fact]
        public async Task AnchovyBay_EcosimOnly_Succeeds()
        {
            // Arrange
            string outputFolder = Path.Combine(
                AppContext.BaseDirectory, "Testoutput", "AnchovyBay", "AnchovyBay_EcosimOnly_runinfo");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, recursive: true);

            // Act
            ConsoleRunResult result = await ConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_EcosimOnly_runinfo.json", "AnchovyBay");

            // Assert
            result.ExitCode.Should().Be(0, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");

            Directory.GetFiles(outputFolder).Should().BeEquivalentTo(
                Path.Combine(outputFolder, "EwERunConsole_log.txt"));

            outputFolder = Path.Combine(result.ActualOutputFolder, "ecosim_New Ecosim scenario");
            result.StdOut.Should().Contain(outputFolder, $"the output folder path {outputFolder} should be mentioned in the console output");

            Directory.GetFiles(outputFolder).Should().BeEquivalentTo(
                Path.Combine(outputFolder, "Biomass_annual.csv"),
                Path.Combine(outputFolder, "Biomass_monthly.csv"),
                Path.Combine(outputFolder, "Catch_annual.csv"),
                Path.Combine(outputFolder, "Catch_monthly.csv"));
        }

        [Fact]
        public async Task AnchovyBay_EcosimAndEcospace_Succeeds()
        {
            // Arrange
            string outputFolder = Path.Combine(
                AppContext.BaseDirectory, "Testoutput", "AnchovyBay", "AnchovyBay_runinfo");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, recursive: true);

            // Act
            ConsoleRunResult result = await ConsoleRunner.RunAsync(
                @"Testdata\AnchovyBay\AnchovyBay_runinfo.json", "AnchovyBay");

            // Assert
            result.ExitCode.Should().Be(0, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");
            Directory.Exists(outputFolder).Should().BeTrue("the run should create its output folder");
            File.Exists(Path.Combine(outputFolder, "EwERunConsole_log.txt"))
                .Should().BeTrue("the console log file should be written to the output folder");

            string ecosimFolder = Path.Combine(outputFolder, "ecosim_New Ecosim scenario");
            string ecospaceFolder = Path.Combine(outputFolder, "BayOfAnchovies");
            string ascFolder = Path.Combine(ecospaceFolder, "asc");
            string csvFolder = Path.Combine(ecospaceFolder, "csv");

            Directory.GetDirectories(outputFolder, "*", SearchOption.AllDirectories)
                .Should().BeEquivalentTo(ecosimFolder, ecospaceFolder, ascFolder, csvFolder);

            Directory.GetFiles(ecosimFolder, "*.csv").Should().BeEquivalentTo(
                Path.Combine(ecosimFolder, "Biomass_annual.csv"),
                Path.Combine(ecosimFolder, "Biomass_monthly.csv"),
                Path.Combine(ecosimFolder, "Catch_annual.csv"),
                Path.Combine(ecosimFolder, "Catch_monthly.csv"));

            Directory.GetFiles(ascFolder, "*.asc").Should()
                .NotBeEmpty("Ecospace should write ASC maps to BayOfAnchovies\\asc");
            Directory.GetFiles(csvFolder, "*.csv").Should()
                .NotBeEmpty("Ecospace should write CSV output to BayOfAnchovies\\csv");
        }

        [Fact]
        public async Task VLIZ_EcosimOnly_Succeeds()
        {
            // Arrange
            string outputFolder = Path.Combine(
                AppContext.BaseDirectory, "Testoutput", "VLIZ", "VLIZ_runinfo");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, recursive: true);

            // Act
            ConsoleRunResult result = await ConsoleRunner.RunAsync(
                @"Testdata\VLIZ\VLIZ_runinfo.json", "VLIZ");

            // Assert
            result.ExitCode.Should().Be(0, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");

            outputFolder = Path.Combine(result.ActualOutputFolder, "ecosim_V15_TechnicalReport");
            result.StdOut.Should().Contain(outputFolder, $"the output folder path {outputFolder} should be mentioned in the console output");

            Directory.GetFiles(outputFolder, "*.csv")
                .Should().NotBeEmpty("Ecosim should write at least one CSV output file");
        }

        [Fact]
        public async Task Andy_R0001_0000_Succeeds()
        {
            // Arrange
            string outputFolder = Path.Combine(
                AppContext.BaseDirectory, "Testoutput", "Andy", "Andy_R0001_0000");

            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, recursive: true);

            // Act
            ConsoleRunResult result = await ConsoleRunner.RunAsync(
                @"Testdata\Andy\R0001_0000.json", "Andy_R0001_0000");

            // Assert
            result.ExitCode.Should().Be(0, because: result.StdOut);
            result.StdOut.Should().Contain("Run completed");
        }
    }
}

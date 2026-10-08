using System;
using System.IO;
using Eii.Ecopath.Runner.Services.Runtime;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eii.Ecopath.Runner.Services.Tests.Services
{
    public class cEcosimModifierServiceTests
    {
        private static string CreateTempRoot()
        {
            string root = Path.Combine(Path.GetTempPath(), "EwERunnerTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return root;
        }

        [Fact]
        public void RelocateLowerCasedOutput_MovesFilesFromLowerCasedTwin()
        {
            // Arrange
            string root = CreateTempRoot();
            try
            {
                string path = Path.Combine(root, "Ecosim_Test");
                string lowered = path.ToLowerInvariant();
                Directory.CreateDirectory(lowered);
                string loweredFile = Path.Combine(lowered, "biomass_annual.csv");
                File.WriteAllText(loweredFile, "year,value");
                var logger = new Mock<ILogger>();

                // Act
                Action act = () => cEcosimModifierService.RelocateLowerCasedOutput(path, logger.Object);

                // Assert
                act.Should().NotThrow();
                File.Exists(Path.Combine(path, "biomass_annual.csv")).Should().BeTrue();
                if (!OperatingSystem.IsWindows())
                {
                    Directory.Exists(lowered).Should().BeFalse("the empty lower-cased twin folder should be removed");
                    Directory.Exists(root).Should().BeTrue("the shared root folder must not be removed");
                }
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
        }

        [Fact]
        public void RelocateLowerCasedOutput_NoTwin_IsNoOp()
        {
            // Arrange
            string root = CreateTempRoot();
            try
            {
                string path = Path.Combine(root, "Ecosim_Test");
                var logger = new Mock<ILogger>();

                // Act
                Action act = () => cEcosimModifierService.RelocateLowerCasedOutput(path, logger.Object);

                // Assert
                act.Should().NotThrow();
                Directory.GetDirectories(root).Should().BeEmpty("no folders should be created when there is nothing to relocate");
            }
            finally
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, true);
            }
        }
    }
}

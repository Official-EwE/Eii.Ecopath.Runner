using System;
using Eii.Ecopath.Runner.Services.Runtime;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Eii.Ecopath.Runner.Services.Tests.Automation.Foundation
{
    public class cFunctionNodeTests
    {
        private static (cTestFunctionNode Node, cTestShapeData Shape, Mock<ILogger> Logger) CreateNode(int nPoints)
        {
            var coreService = new Mock<ICoreService>();
            var logger = new Mock<ILogger>();
            var shape = new cTestShapeData(nPoints);
            var node = new cTestFunctionNode(coreService.Object, shape, logger.Object);
            return (node, shape, logger);
        }

        private static void VerifyWarningLogged(Mock<ILogger> logger, Times times)
        {
            logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

        [Fact]
        public void load_AlwaysReturnsFalseAndCallsShapeUpdate()
        {
            // Arrange
            var (node, shape, _) = CreateNode(5);

            // Act
            var result = node.load("somefile.txt");

            // Assert
            result.Should().BeFalse();
            shape.UpdateCallCount.Should().Be(1);
        }

        [Fact]
        public void set_WithNullPoints_ReturnsFalse()
        {
            // Arrange
            var (node, shape, _) = CreateNode(5);
            var originalData = shape.ShapeData;

            // Act
            var result = node.set(null!);

            // Assert
            result.Should().BeFalse();
            shape.ShapeData.Should().BeEquivalentTo(originalData);
        }

        [Fact]
        public void set_WithFewerPointsThanShape_SetsOnlyProvidedPointsAndIgnoresExcessPositions()
        {
            // Arrange
            var (node, shape, _) = CreateNode(5);

            // Act
            var result = node.set(new float[] { 2f, 4f });

            // Assert
            result.Should().BeTrue();
            shape.ShapeData.Should().Equal(2f, 4f, 1f, 1f, 1f, 1f);
        }

        [Fact]
        public void set_WithMorePointsThanShape_IgnoresExcessPoints()
        {
            // Arrange
            var (node, shape, _) = CreateNode(3);

            // Act
            var result = node.set(new float[] { 1f, 2f, 3f, 4f, 5f });

            // Assert
            result.Should().BeTrue();
            shape.ShapeData.Should().Equal(1f, 2f, 3f, 1f);
        }

        [Fact]
        public void fill_WithNullPoints_ReturnsFalse()
        {
            // Arrange
            var (node, shape, _) = CreateNode(5);
            var originalData = shape.ShapeData;

            // Act
            var result = node.fill(null!);

            // Assert
            result.Should().BeFalse();
            shape.ShapeData.Should().BeEquivalentTo(originalData);
        }

        [Fact]
        public void fill_WithPointsPattern_RepeatsPatternAcrossEntireShape()
        {
            // Arrange
            var (node, shape, _) = CreateNode(5);

            // Act
            var result = node.fill(new float[] { 1f, 2f });

            // Assert
            result.Should().BeTrue();
            shape.ShapeData.Should().Equal(1f, 2f, 1f, 2f, 1f, 1f);
        }

        [Fact]
        public void reshape_WithNullShapeTypeName_ReturnsFalseAndLogsWarning()
        {
            // Arrange
            var (node, _, logger) = CreateNode(5);

            // Act
            var result = node.reshape(null!, new float[] { 1f });

            // Assert
            result.Should().BeFalse();
            VerifyWarningLogged(logger, Times.Once());
        }

        [Fact]
        public void reshape_WithEmptyShapeTypeName_ReturnsFalseAndLogsWarning()
        {
            // Arrange
            var (node, _, logger) = CreateNode(5);

            // Act
            var result = node.reshape(string.Empty, new float[] { 1f });

            // Assert
            result.Should().BeFalse();
            VerifyWarningLogged(logger, Times.Once());
        }

        [Fact]
        public void reshape_WithUnparseableShapeTypeName_ReturnsFalseAndLogsWarning()
        {
            // Arrange
            var (node, _, logger) = CreateNode(5);

            // Act
            var result = node.reshape("NotARealShapeType", new float[] { 1f });

            // Assert
            result.Should().BeFalse();
            VerifyWarningLogged(logger, Times.Once());
        }

        [Fact]
        public void reshape_WithNullParameters_ReturnsFalseAndLogsWarning()
        {
            // Arrange
            var (node, _, logger) = CreateNode(5);

            // Act
            var result = node.reshape("Sigmoid", null!);

            // Assert
            result.Should().BeFalse();
            VerifyWarningLogged(logger, Times.Once());
        }
    }
}

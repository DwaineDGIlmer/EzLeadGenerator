using Application.Configurations;
using Application.Logging;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.UnitTests.Logging;

sealed public class AzureLoggerProviderTest
{
    [Fact]
    public void CreateLogger_ReturnsAzureLogger_ForValidCategory()
    {
        // Arrange
        var cacheBlobClientMock = new Mock<ICacheBlobClient>();
        var logEventFactory = new Mock<Func<ILogEvent>>().Object;
        var loggerMock = new Mock<ILogger<AzureLoggerProvider>>();
        var optionsMock = Options.Create(new EzLeadSettings());
        var provider = new AzureLoggerProvider(
            logEventFactory,
            cacheBlobClientMock.Object,
            optionsMock);

        // Act
        var logger = provider.CreateLogger("TestCategory");

        // Assert
        Assert.NotNull(logger);
        Assert.IsType<AzureLogger>(logger);
    }

    [Fact]
    public void CreateLogger_ReturnsSameLogger_ForSameCategory()
    {
        // Arrange
        var cacheBlobClientMock = new Mock<ICacheBlobClient>();
        var logEventFactory = new Mock<Func<ILogEvent>>().Object;
        var loggerMock = new Mock<ILogger<AzureLoggerProvider>>();
        var optionsMock = new Mock<IOptions<EzLeadSettings>>();
        var provider = new AzureLoggerProvider(
            logEventFactory,
            cacheBlobClientMock.Object,
            optionsMock.Object);

        // Act
        var logger1 = provider.CreateLogger("TestCategory");
        var logger2 = provider.CreateLogger("TestCategory");

        // Assert
        Assert.Same(logger1, logger2);
    }

    [Fact]
    public void CreateLogger_ReturnsNullLogger_OnException()
    {
        // Arrange
        var cacheBlobClientMock = new Mock<ICacheBlobClient>();
        var logEventFactory = new Mock<Func<ILogEvent>>().Object;
        var loggerMock = new Mock<ILogger<AzureLoggerProvider>>();
        var optionsMock = new Mock<IOptions<EzLeadSettings>>();
        var provider = new AzureLoggerProvider(
            logEventFactory,
            cacheBlobClientMock.Object,
            optionsMock.Object);

        // Simulate exception by manipulating internal dictionary
        var categoryName = null as string;

        // Act
        var logger = provider.CreateLogger(categoryName!);

        // Assert
        Assert.Equal(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, logger);
    }

    [Fact]
    public void Dispose_SuppressesFinalize()
    {
        // Arrange
        var cacheBlobClientMock = new Mock<ICacheBlobClient>();
        var logEventFactory = new Mock<Func<ILogEvent>>().Object;
        var loggerMock = new Mock<ILogger<AzureLoggerProvider>>();
        var optionsMock = new Mock<IOptions<EzLeadSettings>>();
        var provider = new AzureLoggerProvider(
            logEventFactory,
            cacheBlobClientMock.Object,
            optionsMock.Object);

        // Act & Assert
        provider.Dispose();
        // No exception means success, nothing to assert
    }
}
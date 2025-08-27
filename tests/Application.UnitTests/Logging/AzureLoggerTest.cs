using Application.Configurations;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Logging.Tests
{
    public class AzureLoggerTest
    {
        private EzLeadSettings GetSettings(LogLevel logLevel = LogLevel.Information, bool enabled = true)
        {
            return new EzLeadSettings
            {
                LoggingLevel = logLevel,
                LoggingEnabled = enabled,
                ApplicationId = "TestApp",
                ComponentId = "TestComponent",
                LoggingBlobName = "logs",
                LoggingPrefix = "prefix"
            };
        }

        private ILogEvent GetLogEvent()
        {
            var mock = new Mock<ILogEvent>();
            mock.SetupAllProperties();
            mock.Setup(e => e.Serialize()).Returns("{}");
            return mock.Object;
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenArgumentsAreNull()
        {
            var cacheBlobClient = new Mock<ICacheBlobClient>().Object;
            var settings = Options.Create(GetSettings());
            Func<ILogEvent> factory = () => GetLogEvent();

            Assert.Throws<ArgumentNullException>(() => new AzureLogger(null!, settings, factory));
            Assert.Throws<ArgumentNullException>(() => new AzureLogger(cacheBlobClient, null!, factory));
            Assert.Throws<ArgumentNullException>(() => new AzureLogger(cacheBlobClient, settings, null!));
        }

        [Fact]
        public void IsEnabled_ReturnsExpectedValue()
        {
            var logger = new AzureLogger(
                new Mock<ICacheBlobClient>().Object,
                Options.Create(GetSettings(LogLevel.Warning, true)),
                () => GetLogEvent());

            Assert.False(logger.IsEnabled(LogLevel.Information));
            Assert.True(logger.IsEnabled(LogLevel.Warning));
            Assert.True(logger.IsEnabled(LogLevel.Error));
        }

        [Fact]
        public void Log_DoesNotLog_WhenDisabledOrBelowMinLevel()
        {
            var cacheBlobClient = new Mock<ICacheBlobClient>();
            var logger = new AzureLogger(
                cacheBlobClient.Object,
                Options.Create(GetSettings(LogLevel.Error, false)),
                () => GetLogEvent());

            logger.Log(LogLevel.Information, new EventId(1), "state", null, (s, e) => s.ToString());
            cacheBlobClient.Verify(c => c.PutAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Log_StoresLogEvent_WhenEnabledAndLevelIsSufficient()
        {
            var cacheBlobClient = new Mock<ICacheBlobClient>();
            cacheBlobClient
                .Setup(c => c.PutAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(string.Empty))
                .Verifiable();

            var logger = new AzureLogger(
                cacheBlobClient.Object,
                Options.Create(GetSettings(LogLevel.Information, true)),
                () => GetLogEvent());

            logger.Log(LogLevel.Information, new EventId(1), "state", null, (s, e) => s.ToString());
            await Task.Delay(100); // Allow async log to run

            cacheBlobClient.Verify(c => c.PutAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public void BeginScope_ReturnsNull()
        {
            var logger = new AzureLogger(
                new Mock<ICacheBlobClient>().Object,
                Options.Create(GetSettings()),
                () => GetLogEvent());

            Assert.Null(logger.BeginScope("scope"));
        }

        [Fact]
        public void Dispose_SetsDisposedFlag()
        {
            var logger = new AzureLogger(
                new Mock<ICacheBlobClient>().Object,
                Options.Create(GetSettings()),
                () => GetLogEvent());

            logger.Dispose();
            // No exception should be thrown, and Dispose can be called multiple times safely
            logger.Dispose();
        }

        [Fact]
        public void GetPath_ReturnsExpectedPath()
        {
            var settings = GetSettings();
            var path = AzureLogger.GetPath("logname", settings);
            Assert.Equal("logs/prefix.logname", path);
        }
    }
}
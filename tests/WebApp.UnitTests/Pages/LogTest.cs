using Application.Configurations;
using Application.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public sealed class LogTest
{
    [Fact]
    public void LogTest_PageModel_CanBeCreated()
    {
        // Arrange & Act
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LogBlobReaderService>>();
        var mockClient = new Mock<BlobContainerClient>();
        var logBlobReaderService = new LogBlobReaderService(mockClient.Object, mockLogger.Object, Options.Create(new EzLeadSettings()));
        var pageModel = new LogModel(logBlobReaderService);

        // Assert
        Assert.NotNull(pageModel);
    }
}
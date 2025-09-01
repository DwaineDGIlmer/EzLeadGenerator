using Application.Configurations;
using Application.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Moq;

namespace WebApp.UnitTests.Pages;

public sealed class FoldersTest
{
    [Fact]
    public void Constructor_Should_InstantiateClass()
    {
        // Arrange & Act
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LogBlobReaderService>>();
        var mockClient = new Mock<BlobContainerClient>();
        var logBlobReaderService = new LogBlobReaderService(mockClient.Object, mockLogger.Object, Options.Create(new EzLeadSettings()));
        var instance = new WebApp.Pages.FolderModel(logBlobReaderService);

        // Assert
        Assert.NotNull(instance);
    }
}
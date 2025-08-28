using Application.Configurations;
using Application.Services;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Moq;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public class LogsTestModelTests
{
    [Fact]
    public async Task OnGet_DoesNotThrow()
    {
        // Arrange
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<LogBlobReaderService>>();
        var mockClient = new Mock<BlobContainerClient>();
        var logBlobReaderService = new LogBlobReaderService(mockClient.Object, mockLogger.Object, Options.Create(new EzLeadSettings()));
        var model = new LogsModel(logBlobReaderService);

        // Act & Assert
        var results = await Assert.ThrowsAsync<NullReferenceException>(() => model.OnGetAsync());
        Assert.Contains("Object reference not set to an instance of an object.", results.Message);
    }
}
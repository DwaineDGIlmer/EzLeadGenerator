using Microsoft.Extensions.Logging;
using Moq;
using WebApp.Pages;

namespace WebApp.UnitTests.Pages;

public sealed class PrivacyModelTests
{
    [Fact]
    public void OnGet_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<PrivacyModel>>();
        var model = new PrivacyModel(mockLogger.Object);

        // Act
        model.OnGet();

        // Assert
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(static (v, t) => v.ToString().Contains("Privacy page accessed.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
using Application.Logging;
using Microsoft.Extensions.Logging;

namespace Application.UnitTests.Logging;

sealed public class LoggingExtensionsTest : UnitTestsBase
{
    [Fact]
    public void UserLoggedIn_LogsWarningWithUserName()
    {
        // Arrange
        var mockLogger = new MockLogger<LoggingExtensionsTest>(LogLevel.Warning);
        string testUserName = "testuser";

        // Act
        mockLogger.UserLoggedIn(testUserName);

        // Assert
        Assert.True(mockLogger.Contains($"[Warning] User logged in: {testUserName}"));
    }
}
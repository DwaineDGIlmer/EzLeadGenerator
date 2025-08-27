using Microsoft.Extensions.Logging;

namespace Application.Logging;

/// <summary>
/// Provides extension methods for logging user-related events.
/// </summary>
/// <remarks>This class contains methods that extend the functionality of <see cref="ILogger"/>  to log specific
/// user-related events, such as user logins. These methods are designed  to simplify logging by providing
/// strongly-typed, pre-defined log messages.</remarks>
public static partial class LoggingExtensions
{
    /// <summary>
    /// Logs a trace message indicating that a user has logged in.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance used to log the message.</param>
    /// <param name="userName">The name of the user who logged in. This value is included in the log message.</param>
    [LoggerMessage(EventId = 3001, Level = LogLevel.Warning, Message = "User logged in: {userName}")]
    public static partial void UserLoggedIn(this ILogger logger, string userName);
}

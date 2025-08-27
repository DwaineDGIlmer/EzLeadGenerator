#nullable disable
using Loggers.Contracts;
using Loggers.Publishers;
using Microsoft.Extensions.Logging;

namespace Application.UnitTests;

/// <summary>
/// Provides a base class for unit tests, offering utility methods and configurations to simplify the setup of test
/// environments.
/// </summary>
/// <remarks>This class includes functionality for initializing shared services, creating mock HTTP
/// clients, and generating test log events. It is designed to streamline common tasks in unit testing scenarios,
/// such as mocking HTTP responses or serializing log events.</remarks>
public class UnitTestsBase
{
    /// <summary>
    /// A mock implementation of the <see cref="ILogger"/> interface for testing purposes.
    /// </summary>
    /// <remarks>This logger captures log messages in an in-memory collection and publishes them using the
    /// provided <see cref="IPublisher"/>. It can be used to verify logging behavior in unit tests by inspecting the
    /// captured messages.</remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MockLogger"/> class with the specified log level and publisher.
    /// </remarks>
    /// <param name="logLevel">The minimum <see cref="LogLevel"/> at which log messages will be processed.</param>
    /// <param name="publisher">The <see cref="IPublisher"/> instance used to publish log messages. Cannot be <see langword="null"/>.</param>
    public class MockLogger<T>(LogLevel logLevel, IPublisher publisher = null) : ILogger<T>
    {
        private readonly LogLevel _logLevel = logLevel;
        private readonly IPublisher _publisher = publisher ?? new ConsolePublisher(20);

        /// <summary>
        /// Gets the collection of messages.
        /// </summary>
        public IList<string> Messages { get; } = [];

        /// <summary>
        /// Determines whether the collection contains any message that includes the specified substring.
        /// </summary>
        /// <param name="message">The substring to search for within the messages. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if at least one message in the collection contains the specified substring;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Contains(string message) => Messages.Any(m => m.Contains(message));

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <remarks>The scope can be used to group a set of operations together, providing contextual
        /// information that can be accessed during the lifetime of the scope. This is commonly used in logging
        /// frameworks to include additional context in log messages.</remarks>
        /// <typeparam name="TState">The type of the state to associate with the scope.</typeparam>
        /// <param name="state">The state to associate with the scope. This can be used to provide contextual information.</param>
        /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope when disposed.</returns>
        public IDisposable BeginScope<TState>(TState state) => null!;

        /// <summary>
        /// Determines whether logging is enabled for the specified log level.
        /// </summary>
        /// <param name="logLevel">The log level to check.</param>
        /// <returns><see langword="true"/> if logging is enabled for the specified <paramref name="logLevel"/>;  otherwise, <see
        /// langword="false"/>.</returns>
        public bool IsEnabled(LogLevel logLevel) => logLevel >= _logLevel;

        /// <summary>
        /// Logs a formatted message at the specified log level.
        /// </summary>
        /// <remarks>The method logs the message only if the specified <paramref name="logLevel"/> is
        /// enabled. The formatted message is added to the internal message collection and written to the
        /// publisher.</remarks>
        /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
        /// <param name="logLevel">The severity level of the log message.</param>
        /// <param name="eventId">The identifier for the event being logged.</param>
        /// <param name="state">The state object containing information to be logged.</param>
        /// <param name="exception">The exception related to the log entry, or <see langword="null"/> if no exception is associated.</param>
        /// <param name="formatter">A function that formats the <paramref name="state"/> and <paramref name="exception"/> into a log message
        /// string.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var message = $"[{logLevel}] {formatter(state, exception)}";
                Messages.Add(message);
                _publisher.WriteLine(message);
            }
        }
    }

}

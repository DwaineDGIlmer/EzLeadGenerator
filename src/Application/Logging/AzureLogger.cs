using Application.Configurations;
using Application.Constants;
using Core.Contracts;
using Core.Helpers;
using Core.Models;
using Loggers.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Application.Logging;

/// <summary>
/// Provides a logger implementation that integrates with Azure Blob Storage for storing structured log events.
/// </summary>
/// <remarks>The <see cref="AzureLogger"/> class implements the <see cref="ILogger"/> interface and
/// supports structured logging with features such as log level filtering, scoped logging, and asynchronous storage
/// of log events in Azure Blob Storage. It is designed for applications that require persistent, cloud-based
/// logging with enhanced context management.</remarks>
public class AzureLogger : ILogger, IDisposable
{
    /// <summary>
    /// Represents a client used for interacting with cached blob storage.
    /// </summary>
    /// <remarks>This field is intended to provide access to an implementation of <see
    /// cref="ICacheBlobClient"/>  for managing cached blob operations. It is a readonly dependency and cannot be
    /// modified after initialization.</remarks>
    private readonly ICacheBlobClient _cacheBlobClient;

    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    /// <remarks>This field is used internally to track the disposal state of the object.  It should
    /// not be accessed directly outside of the class.</remarks>
    private bool _disposed;

    #region Public Properties
    /// <summary>
    /// The settings used for configuring the logger. 
    /// </summary>
    internal EzLeadSettings Settings { get; }

    /// <summary>
    /// Indicates whether the feature is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets the minimum <see cref="LogLevel"/> that will be logged by the logger.
    /// Messages with a lower log level will be ignored.
    /// </summary>
    /// <remarks>
    /// The <see cref="ApplicationLogger"/> class allows logging messages with different log levels, supports scoping for logical operations,
    /// and integrates with external scope providers for enhanced context management. It is designed to be used in applications that require
    /// structured and scoped logging.
    /// </remarks>
    public LogLevel MinLogLevel { get; }

    /// <summary>
    /// Gets the factory method used to create log events.
    /// </summary>  
    internal Func<ILogEvent> LogEventFactory { get; }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureLogger"/> class with the specified settings, log event
    /// factory, and category name.
    /// </summary>
    /// <param name="cacheBlobClient">The cache blob client used for storing log events. Cannot be null.</param>
    /// <param name="settings">The application settings containing logging configuration. Cannot be null.</param>
    /// <param name="factory">A factory function used to create instances of <see cref="ILogEvent"/>. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="settings"/>, <paramref name="factory"/> is null.</exception>
    public AzureLogger(
        ICacheBlobClient cacheBlobClient,
        IOptions<EzLeadSettings> settings,
        Func<ILogEvent> factory)
    {
        Settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        MinLogLevel = Settings.LoggingLevel;
        Enabled = Settings.LoggingEnabled;
        Settings.Environment = Environment.GetEnvironmentVariable(Defaults.AspNetCoreEnvironment) ?? "Production";

        _cacheBlobClient = cacheBlobClient ?? throw new ArgumentNullException(nameof(cacheBlobClient));

        LogEventFactory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <remarks>Scopes are used to group related log messages or operations. They are typically used
    /// in  structured logging to provide additional context for log entries.</remarks>
    /// <typeparam name="TState">The type of the state to associate with the scope. This type must be non-nullable.</typeparam>
    /// <param name="state">The state object to associate with the scope. This object provides contextual information that can be used
    /// by loggers or other components during the scope's lifetime.</param>
    /// <returns>An <see cref="IDisposable"/> that ends the scope when disposed. Returns <see langword="null"/>  if the scope is not supported.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    /// <summary>
    /// Determines whether logging is enabled for the specified <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>
    /// <c>true</c> if the specified <paramref name="logLevel"/> is greater than or equal to the configured minimum log level; otherwise, <c>false</c>.
    /// </returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        // Only enable log levels greater than or equal to the configured LogLevel
        return logLevel >= MinLogLevel && Enabled;
    }

    /// <summary>
    /// Logs an event with the specified log level, event ID, state, exception, and formatter.
    /// </summary>
    /// <remarks>This method creates a log event and populates it with the provided details, including
    /// metadata such as the timestamp, application ID, component ID, and correlation information. The log event is
    /// only created and processed if the specified <paramref name="logLevel"/> is enabled for the logger.</remarks>
    /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
    /// <param name="logLevel">The severity level of the log message.</param>
    /// <param name="eventId">The identifier for the log event.</param>
    /// <param name="state">The state object containing the log message or additional context.</param>
    /// <param name="exception">The exception associated with the log event, if any. Can be <see langword="null"/>.</param>
    /// <param name="formatter">A function that formats the <paramref name="state"/> and <paramref name="exception"/> into a log message.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        _ = Task.Run(() => LogAsync(logLevel, eventId, state, exception, formatter));
    }

    /// <summary>
    /// Asynchronously logs an event with the specified details, including log level, event ID, state, exception, and a
    /// custom formatter.
    /// </summary>
    /// <remarks>This method creates a log event, populates its properties with the provided details, and
    /// writes the serialized log entry to blob storage. If an error occurs during the logging process, the method
    /// disables further logging by setting the <c>Enabled</c> property to <see langword="false"/>.</remarks>
    /// <typeparam name="TState">The type of the state object to be logged.</typeparam>
    /// <param name="logLevel">The severity level of the log entry.</param>
    /// <param name="eventId">A unique identifier for the event being logged.</param>
    /// <param name="state">The state object containing contextual information about the log entry.</param>
    /// <param name="exception">An optional exception associated with the log entry. Can be <see langword="null"/> if no exception is present.</param>
    /// <param name="formatter">A function that formats the <paramref name="state"/> and <paramref name="exception"/> into a log message string.
    /// The function must not return <see langword="null"/>.</param>
    /// <returns>A task that represents the asynchronous logging operation.</returns>
    public async Task LogAsync<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        try
        {
            // Create a new log event using the factory method
            var logEvent = LogEventFactory();

            // Populate other logEvent properties as needed
            logEvent.Id = DateTime.UtcNow.Ticks.ToString();
            logEvent.Body = formatter(state, exception);
            if (exception is not null)
            {
                logEvent.Body += $"{Environment.NewLine}Exception: {exception.GetType().FullName}: {exception.Message}{Environment.NewLine}{exception.StackTrace}";
            }
            logEvent.Timestamp = DateTimeOffset.UtcNow;
            logEvent.ApplicationId = Settings.ApplicationId ?? string.Empty;
            logEvent.ComponentId = Settings.ComponentId ?? string.Empty;
            logEvent.Environment = Settings.Environment ?? string.Empty;
            logEvent.Level = logLevel;
            logEvent.Tags = logEvent.Tags ?? new Dictionary<string, string>();
            logEvent.Tags.Add("EventId", eventId.Id.ToString());
            logEvent.Exception = exception is not null ? new SerializableException(exception) : null;
            logEvent.CorrelationId = Activity.Current?.RootId ?? Guid.NewGuid().ToString();
            logEvent.TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty;
            logEvent.SpanId = Activity.Current?.SpanId.ToString() ?? string.Empty;
            logEvent.InnerExceptions = ExceptionHelper.GetInnerExceptions(exception);

            var loggedEvent = logEvent.Serialize();
            await _cacheBlobClient.PutAsync(GetPath($"{logEvent.Level}.{logEvent.Id}", Settings), System.Text.Encoding.UTF8.GetBytes(logEvent.Serialize()));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to write log event to blob storage: {ex}");
            Enabled = false;
        }
    }

    /// <summary>
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <remarks>This method should be called when the instance is no longer needed to ensure proper
    /// cleanup of resources. Once disposed, the instance should not be used further.</remarks>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Constructs the full path for a log file based on the specified log name.
    /// </summary>
    /// <remarks>The resulting path ensures that there are no leading or trailing slashes. The log
    /// name  is appended to the logging prefix, which is combined with the logging blob name.</remarks>
    /// <param name="logName">The name of the log file, without any path or extension.</param>
    /// <param name="settings">The application settings containing logging configuration. Cannot be null.</param>
    /// <returns>A string representing the full path of the log file, combining the logging blob name,  logging prefix, and
    /// the specified log name.</returns>
    public static string GetPath(string logName, EzLeadSettings settings)
    {
        return $"/{$"{DateTime.UtcNow.Month}_{DateTime.UtcNow.Day}_{DateTime.UtcNow.Year}"}.{logName}.{settings.LoggingBlobName}".TrimStart('/');
    }
}

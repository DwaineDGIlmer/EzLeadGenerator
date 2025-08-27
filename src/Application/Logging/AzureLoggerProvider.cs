using Application.Configurations;
using Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Application.Logging;

/// <summary>
/// Provides a logger implementation that integrates with Azure-based logging mechanisms.
/// </summary>
/// <remarks>This class is designed to create loggers that interact with Azure services, leveraging the provided
/// <see cref="ICacheBlobClient"/> for any necessary Azure storage or caching operations. It implements the <see
/// cref="ILoggerProvider"/> interface, allowing it to be used in .NET logging frameworks.</remarks>
public class AzureLoggerProvider : ILoggerProvider
{
    private readonly ICacheBlobClient _cacheBlobClient;
    private readonly ILogger<AzureLoggerProvider> _logger;
    private readonly IOptions<EzLeadSettings> _options;
    private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
    private readonly Func<ILogEvent> _logEventFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureLoggerProvider"/> class.
    /// </summary>
    /// <param name="func">A factory function that creates instances of <see cref="ILogEvent"/>.</param>
    /// <param name="cacheBlobClient">The client used to interact with the cache blob storage.</param>
    /// <param name="options">The configuration options for the logger provider.</param>
    public AzureLoggerProvider(
        Func<ILogEvent> func,
        ICacheBlobClient cacheBlobClient,
        IOptions<EzLeadSettings> options)
    {
        _cacheBlobClient = cacheBlobClient;
        _options = options;
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AzureLoggerProvider>();
        _logEventFactory = func;
    }

    /// <summary>
    /// Creates and returns a logger instance for the specified category name.
    /// </summary>
    /// <remarks>This method ensures that loggers are cached and reused for the same category name. If a
    /// logger for the specified  category name already exists, the existing instance is returned. Otherwise, a new
    /// logger is created and added to the cache.</remarks>
    /// <param name="categoryName">The name of the category for messages produced by the logger. Cannot be null or empty.</param>
    /// <returns>An <see cref="ILogger"/> instance associated with the specified category name. If the logger cannot be created,
    /// a  <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger"/> instance is returned.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        try
        {
            if (_loggers.TryGetValue(categoryName, out var existingLogger))
            {
                return existingLogger;
            }

            var logger = new AzureLogger(_cacheBlobClient, _options, _logEventFactory);
            _loggers.TryAdd(categoryName, logger);
            return logger;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create logger for category: {CategoryName}", categoryName);
            return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="AzureLoggerProvider"/>.
    /// </summary>
    public void Dispose()
    {
        // No resources to dispose
        GC.SuppressFinalize(this);
    }
}

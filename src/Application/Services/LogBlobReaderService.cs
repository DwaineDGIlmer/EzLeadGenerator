using Application.Configurations;
using Azure.Storage.Blobs;
using Core.Contracts;
using Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Services;

/// <summary>
/// Provides functionality for interacting with log files stored in an Azure Blob Storage container.
/// </summary>
/// <remarks>This service is designed to facilitate the retrieval and processing of log files stored in a specific
/// folder structure within an Azure Blob Storage container. It supports operations such as listing folders, listing log
/// files within a folder, and reading log file contents. The service relies on a logging prefix and blob name
/// configuration, which are provided through the <see cref="IOptions{TOptions}"/> parameter during
/// initialization.</remarks>
public class LogBlobReaderService
{
    private readonly string _loggingPrefix;
    private const string _delimiter = "/";
    private readonly BlobContainerClient _containerClient;
    ILogger<LogBlobReaderService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogBlobReaderService"/> class.
    /// </summary>
    /// <param name="containerClient">The <see cref="BlobContainerClient"/> used to interact with the Azure Blob Storage container.</param>
    /// <param name="logger">The logger instance for logging information and errors related to the service operations.</param>
    /// <param name="options">The configuration options for the service, encapsulated in an <see cref="IOptions{TOptions}"/> object. The
    /// options must contain valid settings for logging, including the logging prefix and blob name.</param>
    public LogBlobReaderService(
        BlobContainerClient containerClient,
        ILogger<LogBlobReaderService> logger,
        IOptions<EzLeadSettings> options)
    {
        options.IsNullThrow(nameof(options));
        options.Value.IsNullThrow(nameof(options.Value));
        containerClient.IsNullThrow(nameof(containerClient));

        _logger = logger;
        _loggingPrefix = options.Value.LoggingPrefix;
        _containerClient = containerClient;
    }

    /// <summary>
    /// Asynchronously retrieves a list of folder names from the container, based on a specified prefix and delimiter.
    /// </summary>
    /// <remarks>This method enumerates blobs in the container using the prefix "cache/" and the delimiter
    /// "/". Only folder names (prefixes) are included in the result. Duplicate folder names are removed.</remarks>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of unique folder names.</returns>
    public async Task<List<string>> ListFoldersAsync()
    {
        var result = new HashSet<string>();
        await foreach (var blob in _containerClient.GetBlobsByHierarchyAsync(delimiter: _delimiter))
        {
            if (blob.IsPrefix)
            {
                var folder = blob.Prefix.TrimEnd('/').Split('/').Last();
                result.Add(folder);
            }
        }
        return [.. result];
    }

    /// <summary>
    /// Asynchronously retrieves a list of log file names from the specified folder.
    /// </summary>
    /// <remarks>The method searches for log files within the specified folder using a predefined prefix
    /// format. Only the file names are returned, excluding the folder path.</remarks>
    /// <param name="folder">The name of the folder to search for log files. This value cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of log file names found in
    /// the specified folder. If no files are found, the list will be empty.</returns>
    public async Task<List<string>> ListLogFilesAsync(string folder)
    {
        var prefix = $"{_loggingPrefix}/{folder}/";
        var files = new List<string>();
        await foreach (var blob in _containerClient.GetBlobsAsync(prefix: _loggingPrefix))
        {
            var fileName = blob.Name[(blob.Name.LastIndexOf('/') + 1)..];
            files.Add(fileName);
        }
        return files;
    }

    /// <summary>
    /// Asynchronously reads a log file from the specified folder and file name, and deserializes it into an <see
    /// cref="ILogEvent"/> object.
    /// </summary>
    /// <remarks>The method assumes that the log file is stored in a specific folder structure within the
    /// storage container  and that the file content is in JSON format. Ensure that the file exists and is accessible
    /// before calling this method.</remarks>
    /// <param name="folder">The name of the folder containing the log file. This value cannot be null or empty.</param>
    /// <param name="file">The name of the log file to read. This value cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is an <see cref="ILogEvent"/> object 
    /// deserialized from the log file, or <see langword="null"/> if the file content cannot be deserialized.</returns>
    public async Task<LogEvent?> ReadLogAsync(string folder, string file)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient($"{folder}/{file}");
            var response = await blobClient.DownloadContentAsync();
            var bytes = response.Value.Content.ToArray();

            using var ms = new MemoryStream(bytes);
            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
            using var reader = new StreamReader(gzip);
            var json = reader.ReadToEnd();

            return JsonSerializer.Deserialize<LogEvent>(json);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument provided for folder '{Folder}' or file '{File}'", folder, file);
        }
        return default;
    }
}

/// <summary>
/// Represents a log event with detailed properties mapped to JSON.
/// </summary>
public class LogEvent
{
    /// <summary>
    /// The timestamp of the log event.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = 0;

    /// <summary>
    /// The severity text (e.g., "WARNING").
    /// </summary>
    [JsonPropertyName("severity_text")]
    public string SeverityText { get; set; } = string.Empty;

    /// <summary>
    /// The severity number (e.g., 3).
    /// </summary>
    [JsonPropertyName("severity_number")]
    public int SeverityNumber { get; set; } = 0;

    /// <summary>
    /// The main message or body of the log event.
    /// </summary>
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// The trace identifier.
    /// </summary>
    [JsonPropertyName("trace_id")]
    public string TraceId { get; set; } = string.Empty;

    /// <summary>
    /// The span identifier.
    /// </summary>
    [JsonPropertyName("span_id")]
    public string SpanId { get; set; } = string.Empty;

    /// <summary>
    /// Additional attributes for the log event.
    /// </summary>
    [JsonPropertyName("attributes")]
    public LogAttributes Attributes { get; set; } = new LogAttributes();
}

/// <summary>
/// Represents additional attributes for a log event.
/// </summary>
public class LogAttributes
{
    /// <summary>
    /// The correlation identifier.
    /// </summary>
    [JsonPropertyName("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;
}
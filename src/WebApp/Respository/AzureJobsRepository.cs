using Application.Configurations;
using Application.Constants;
using Application.Contracts;
using Application.Models;
using Azure;
using Azure.Data.Tables;
using Core.Contracts;
using Microsoft.Extensions.Options;
using System.Text.Json;
using WebApp.Extensions;

namespace WebApp.Respository;

/// <summary>
/// Provides an implementation of <see cref="IJobsRepository"/> that uses Azure Table Storage and Blob Storage.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AzureJobsRepository"/> class with the specified table
/// client, blob container client, and logger.
/// </remarks>
/// <param name="tableClient">The <see cref="TableClient"/> used to interact with the Azure Table storage.</param>
/// <param name="cachingService">The <see cref="ICacheService"/> used for caching company profiles.</param>
/// <param name="options">Configuration settings for Azure services, including connection strings and other relevant settings.</param>
/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance used for logging operations within the repository.</param>
public class AzureJobsRepository(
    TableClient tableClient,
    ICacheService cachingService,
    IOptions<AzureSettings> options,
    ILogger<AzureJobsRepository> logger) : IJobsRepository
{
    private readonly ICacheService _cachingService = cachingService ?? throw new ArgumentNullException(nameof(cachingService));
    private readonly TableClient _tableClient = tableClient ?? throw new ArgumentNullException(nameof(tableClient));
    private readonly string _partionKey = options?.Value.JobSummaryPartionKey ?? Defaults.JobSummaryPartionKey;
    private readonly int _cacheExpirationInMinutes = options?.Value?.CacheExpirationInMinutes ?? Defaults.CacheExpirationInMinutes;
    private readonly ILogger<AzureJobsRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Asynchronously retrieves a specific job by its ID.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to retrieve. Cannot be null or empty.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the <see cref="JobSummary"/>
    /// object if found.</returns>
    /// <exception cref="ArgumentException">Thrown when jobId is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the job is not found.</exception>
    public async Task<JobSummary?> GetJobsAsync(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(jobId));

        _logger.LogInformation("Retrieving job for {JobId}", jobId);

        var cachedJob = await _cachingService.GetJobAsync(jobId, _logger);
        if (cachedJob != null)
        {
            return cachedJob;
        }

        try
        {
            var response = await _tableClient.GetEntityAsync<TableEntity>(_partionKey, jobId);
            var json = response.Value.GetString("Data");

            if (string.IsNullOrEmpty(json))
            {
                _logger.LogInformation("Job data is empty for {JobId}", jobId);
                return null;
            }

            var job = JsonSerializer.Deserialize<JobSummary>(json, _options);
            if (job is not null)
            {
                _logger.LogInformation("Retrieved job from cache for {JobId}", jobId);
                await _cachingService.CreateEntryAsync(jobId, job, TimeSpan.FromMinutes(_cacheExpirationInMinutes));
            }
            return job ?? null;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Job not found for {JobId}", jobId);
            return null;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error retrieving job for {JobId}", jobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving job for {JobId}", jobId);
            throw new InvalidOperationException($"An error occurred while retrieving the job with ID: {jobId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a list of job summaries posted on or after the specified date.
    /// </summary>
    /// <param name="fromDate">The date from which to start retrieving job summaries. Only jobs posted on or after this date will be
    /// included.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="JobSummary"/>
    /// objects for the specified date range.</returns>
    public async Task<IEnumerable<JobSummary>> GetJobsAsync(DateTime fromDate)
    {
        _logger.LogInformation("Retrieving jobs posted since {FromDate}", fromDate);

        var cachedJobs = await _cachingService.GetJobsAsync(fromDate, _logger);
        if (cachedJobs != null)
        {
            return cachedJobs;
        }

        var queryResults = _tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{_partionKey}'");
        var jobs = new List<JobSummary>();

        await foreach (var entity in queryResults)
        {
            try
            {
                var json = entity.GetString("Data");
                if (string.IsNullOrEmpty(json)) continue;

                var job = JsonSerializer.Deserialize<JobSummary>(json, _options);
                if (job != null && job.PostedDate >= fromDate)
                {
                    jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse job from Azure Table for entity with RowKey: {RowKey}", entity.RowKey);
            }
        }

        if (jobs.Count != 0)
        {
            _logger.LogInformation("Found {JobCount} jobs posted since {FromDate}", jobs.Count, fromDate);
            await _cachingService.AddJobsAsync(jobs, fromDate, _cacheExpirationInMinutes, _logger);
        }
        return [.. jobs.OrderByDescending(j => j.PostedDate)];
    }

    /// <summary>
    /// Asynchronously saves the specified job summary to the data store.
    /// </summary>
    /// <param name="job">The <see cref="JobSummary"/> object containing the job details to be saved. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when job ID is null or empty.</exception>
    public async Task AddJobAsync(JobSummary job)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (string.IsNullOrWhiteSpace(job.JobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        try
        {
            _logger.LogDebug("Adding job for {JobId}", job.JobId);


            var entity = CreateTableEntity(job, _partionKey, _options);
            await _tableClient.UpsertEntityAsync(entity);
            _logger.LogInformation("Successfully added job for {JobId}", job.JobId);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error adding job for {JobId}", job.JobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding job for {JobId}", job.JobId);
            throw new InvalidOperationException($"An error occurred while adding the job with ID: {job.JobId}", ex);
        }
    }

    /// <summary>
    /// Updates the job information asynchronously based on the provided job summary.
    /// </summary>
    /// <param name="job">The <see cref="JobSummary"/> object containing the updated job details.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when job ID is null or empty.</exception>
    public async Task UpdateJobAsync(JobSummary job)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (string.IsNullOrWhiteSpace(job.JobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        try
        {
            _logger.LogDebug("Updating job for {JobId}", job.JobId);
            var entity = CreateTableEntity(job, _partionKey, _options);

            try
            {
                var existing = await _tableClient.GetEntityAsync<TableEntity>(_partionKey, job.JobId);
                entity["CreatedAt"] = existing.Value.GetString("CreatedAt") ?? DateTime.UtcNow.ToString("o");
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // If entity doesn't exist, set CreatedAt to now
                entity["CreatedAt"] = DateTime.UtcNow.ToString("o");
            }

            await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
            _logger.LogInformation("Successfully updated job for {JobId}", job.JobId);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Job not found for update: {JobId}", job.JobId);
            throw new InvalidOperationException($"Job not found for ID: {job.JobId}", ex);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error updating job for {JobId}", job.JobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating job for {JobId}", job.JobId);
            throw new InvalidOperationException($"An error occurred while updating the job with ID: {job.JobId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously deletes the specified job.
    /// </summary>
    /// <param name="job">The <see cref="JobSummary"/> representing the job to be deleted. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when job ID is null or empty.</exception>
    public async Task DeleteJobAsync(JobSummary job)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (string.IsNullOrWhiteSpace(job.JobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        try
        {
            _logger.LogDebug("Deleting job for {JobId}", job.JobId);

            await _tableClient.DeleteEntityAsync(_partionKey, job.JobId, ETag.All);
            _logger.LogInformation("Successfully deleted job for {JobId}", job.JobId);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Job not found for deletion: {JobId}", job.JobId);
            // Silently succeed when entity doesn't exist for delete operations
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error deleting job for {JobId}", job.JobId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting job for {JobId}", job.JobId);
            throw new InvalidOperationException($"An error occurred while deleting the job with ID: {job.JobId}", ex);
        }
    }

    /// <summary>
    /// Creates a <see cref="TableEntity"/> instance representing the specified job summary.
    /// </summary>
    /// <remarks>The returned <see cref="TableEntity"/> includes key-value pairs representing the job
    /// summary's properties, such as job title, company name, location, and other relevant details. The "Data" field
    /// contains the serialized job object, and timestamps for creation and update are automatically added.</remarks>
    /// <param name="job">The job summary object containing details about the job.</param>
    /// <param name="partionKey">The partition key to associate with the table entity. Cannot be null or empty.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> used to serialize the job data.</param>
    /// <returns>A <see cref="TableEntity"/> populated with the job summary data, including serialized job details and additional
    /// metadata such as timestamps.</returns>
    public static TableEntity CreateTableEntity(JobSummary job, string partionKey, JsonSerializerOptions options)
    {
        return new TableEntity(partionKey, job.JobId)
        {
            {"Data", JsonSerializer.Serialize(job, options)},
            {"Id", job.Id ?? string.Empty},
            {"JobTitle", job.JobTitle ?? string.Empty},
            {"CompanyId", job.CompanyId ?? string.Empty},
            {"CompanyName", job.CompanyName ?? string.Empty},
            {"HiringAgency", job.HiringAgency ?? string.Empty},
            {"Location", job.Location ?? string.Empty},
            {"Division", job.Division ?? string.Empty},
            {"Confidence", job.Confidence},
            {"Reasoning", job.Reasoning ?? string.Empty},
            {"SourceLink", job.SourceLink ?? string.Empty},
            {"SourceName", job.SourceName ?? string.Empty},
            {"PostedDate", job.PostedDate.ToString("o")},
            {"UpdatedAt", DateTime.UtcNow.ToString("o")},
            {"CreatedAt", DateTime.UtcNow.ToString("o")}
        };
    }
}
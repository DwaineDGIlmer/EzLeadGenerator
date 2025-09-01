using Application.Configurations;
using Application.Contracts;
using Application.Models;
using Core.Contracts;
using Core.Helpers;
using System.Reflection;
using System.Text.Json.Serialization;
using WebApp.Extensions;

namespace WebApp.Respository;

/// <summary>
/// Provides a local file-based implementation of the <see cref="IJobsRepository"/> interface for managing job profiles.
/// </summary>
/// <remarks>This class handles the storage and retrieval of job profiles using JSON files in a specified
/// directory. It supports operations such as adding, updating, retrieving, and deleting job profiles. The directory for
/// storing job profiles is specified through configuration options, and the class ensures that this directory exists
/// upon initialization. Logging is used to record operations and errors.</remarks>
sealed public class LocalJobsRepositoryStore : IJobsRepository
{
    private readonly string _jobsProfileDirectory;
    private readonly int _cacheExpirationInHours;
    private readonly ICacheService _cachingService;
    private readonly ILogger<LocalJobsRepositoryStore> _logger;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(), new DateTimeConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalJobsRepositoryStore"/> class.
    /// </summary>
    /// <remarks>This constructor ensures that the specified directories exist by creating them if
    /// they do not.</remarks>
    /// <param name="options">Configuration options for the service, including paths for job and job directories. Cannot be <see langword="null"/>.</param>
    /// <param name="cacheService">The cache service used for caching operations. Cannot be <see langword="null"/>.</param> 
    /// <param name="logger">The logger instance used for logging operations within the store. Cannot be <see langword="null"/>.</param>
    public LocalJobsRepositoryStore(
        IOptions<EzLeadSettings> options,
        ICacheService cacheService,
        ILogger<LocalJobsRepositoryStore> logger)
    {
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(options.Value.FileJobProfileDirectory, nameof(options.Value.FileJobProfileDirectory));

        _cachingService = cacheService;
        _cacheExpirationInHours = options.Value.JobsCacheExpirationInHours;
        _jobsProfileDirectory = GetJobsDirectory(options.Value).Replace('/', '\\');
        _logger = logger;

        // This is the same directory used by any caller using SerpApiSettings
        Directory.CreateDirectory(_jobsProfileDirectory);
    }

    /// <summary>
    /// Asynchronously retrieves the job job for the specified job identifier.
    /// </summary>
    /// <remarks>This method attempts to read a job job from a JSON file located in a predefined
    /// directory. If the file does not exist, a default job summary with unknown values is returned. If the file exists
    /// but cannot be deserialized, an <see cref="InvalidDataException"/> is thrown.</remarks>
    /// <param name="jobId">The unique identifier of the job whose job is to be retrieved.</param>
    /// <returns>A <see cref="JobSummary"/> object containing the job details. If the job job is not found, returns a default
    /// <see cref="JobSummary"/> with unknown values.</returns>
    /// <exception cref="InvalidDataException">Thrown if the deserialized job job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when jobId is null or empty.</exception>
    public async Task<JobSummary?> GetJobAsync(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(jobId));

        _logger.LogInformation("Retrieving job for {JobId}", jobId);

        var cachedJob = await _cachingService.GetJobAsync(jobId, _logger);
        if (cachedJob != null)
        {
            return cachedJob;
        }

        string path = GetFilePath(jobId, _jobsProfileDirectory);
        if (!File.Exists(path))
        {
            _logger.LogInformation("Profile not found for job: {JobId}", jobId);
            return null;
        }

        try
        {
            using FileStream stream = File.OpenRead(path);
            var job = await JsonSerializer.DeserializeAsync<JobSummary>(stream, _options);
            if (job is not null)
            {
                _logger.LogDebug("Creating job for cache for {JobId}", jobId);
                await _cachingService.AddJobAsync(job, _cacheExpirationInHours);
            }
            return job ?? null;
        }
        catch (Exception ex) when (ex is not InvalidDataException)
        {
            _logger.LogError(ex, "Failed to load job for job Id: {JobId}", jobId);
            throw new InvalidOperationException($"Failed to load job job for ID: {jobId}", ex);
        }
    }

    /// <summary>
    /// Asynchronously retrieves a list of job summaries posted on or after the specified date.
    /// </summary>
    /// <remarks>This method reads job summary data from JSON files located in a specified directory. It
    /// filters the jobs based on the provided <paramref name="fromDate"/> and returns jobs ordered by posting date. 
    /// If a file cannot be read or deserialized, an error is logged, and the file is skipped.</remarks>
    /// <param name="fromDate">The date from which to filter job postings. Only jobs posted on or after this date are considered.</param>
    /// <returns>A list of <see cref="JobSummary"/> objects representing jobs that meet the date criteria, ordered by posting date descending.</returns>
    public async Task<IEnumerable<JobSummary>> GetJobsAsync(DateTime fromDate)
    {
        _logger.LogInformation("Retrieving jobs posted since {FromDate}", fromDate);

        var cachedJobs = await _cachingService.GetJobsAsync(fromDate, _logger);
        if (cachedJobs != null)
        {
            return cachedJobs;
        }

        var jobs = new List<JobSummary>();
        if (!Directory.Exists(_jobsProfileDirectory))
        {
            _logger.LogWarning("Jobs directory does not exist: {Directory}", _jobsProfileDirectory);
            return jobs;
        }

        foreach (var file in Directory.GetFiles(_jobsProfileDirectory, "job.*.json"))
        {
            try
            {
                using FileStream stream = File.OpenRead(file);
                var profile = await JsonSerializer.DeserializeAsync<JobSummary>(stream, _options);
                if (profile != null && profile.PostedDate >= fromDate)
                {
                    jobs.Add(profile);
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error in file: {File}", file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading job summary file: {File}", file);
            }
        }

        if (jobs.Count != 0)
        {
            await _cachingService.AddJobsAsync(jobs, fromDate, _cacheExpirationInHours);
        }
        return jobs;
    }

    /// <summary>
    /// Asynchronously adds a job job by saving it to a file in JSON format.
    /// </summary>
    /// <remarks>The job job is saved in a directory specified by the job job directory, with the
    /// filename based on the job's ID. If an error occurs during the save operation, the exception is logged and
    /// rethrown.</remarks>
    /// <param name="job">The job job to be saved. Cannot be <see langword="null"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when job ID is null or empty.</exception>
    public async Task AddJobAsync(JobSummary job)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));

        if (string.IsNullOrWhiteSpace(job.JobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        string path = GetFilePath(job.JobId, _jobsProfileDirectory);
        try
        {
            using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, job, _options);
            _logger.LogInformation("JobSummary saved: {JobId} as {fileName}", job.JobId, Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save job job: {JobId}", job.JobId);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously updates a job job by saving it to a file in JSON format.
    /// </summary>
    /// <remarks>If the job job doesn't exist, it will be created. The UpdatedAt timestamp is automatically set.</remarks>
    /// <param name="job">The job job to be updated. Cannot be <see langword="null"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when job ID is null or empty.</exception>
    public async Task UpdateJobAsync(JobSummary job)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));

        if (string.IsNullOrWhiteSpace(job.JobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        string path = GetFilePath(job.JobId, _jobsProfileDirectory);
        if (!File.Exists(path))
        {
            _logger.LogInformation("Job job not found, creating new: {JobId}", job.JobId);
            await AddJobAsync(job);
            return;
        }

        try
        {
            JobSummary? jobSummary = await UpdateProperties(job, _jobsProfileDirectory, _options, _logger);
            if (jobSummary == null)
            {
                _logger.LogWarning("Profile not found for update: {JobId}", job.JobId);
                return;
            }

            // Save the updated job
            using (FileStream writeStream = File.Create(path))
            {
                await JsonSerializer.SerializeAsync(writeStream, jobSummary ?? job, _options);
            }
            _logger.LogInformation("Job job updated: {JobId}", job.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job job: {JobId}", job.JobId);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously deletes the specified job job from the storage directory.
    /// </summary>
    /// <param name="job">The job job to delete. Cannot be <see langword="null"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when job is null.</exception>
    /// <exception cref="ArgumentException">Thrown when job ID is null or empty.</exception>
    public async Task DeleteJobAsync(JobSummary job)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));

        if (string.IsNullOrWhiteSpace(job.JobId))
            throw new ArgumentException("Job ID cannot be null or empty.", nameof(job));

        string path = GetFilePath(job.JobId, _jobsProfileDirectory);
        await Task.Run(() =>
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    _logger.LogInformation("Job job deleted: {JobId}", job.JobId);
                }
                else
                {
                    _logger.LogWarning("Job job not found for deletion: {JobId}", job.JobId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete job job: {JobId}", job.JobId);
                throw;
            }
        });
    }

    /// <summary>
    /// Updates the properties of a job job by merging them with the existing job job in the specified directory.
    /// </summary>
    /// <param name="job">The job summary to update</param>
    /// <param name="jobDirectory">The jobs directory</param>
    /// <param name="options">The JSON serializer options to use for serialization and deserialization.</param>
    /// <param name="logger">The logger.</param>
    /// <returns></returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    public static async Task<JobSummary?> UpdateProperties(JobSummary job, string jobDirectory, JsonSerializerOptions options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(job, nameof(job));
        ArgumentNullException.ThrowIfNull(jobDirectory, nameof(jobDirectory));

        if (!Directory.Exists(jobDirectory))
        {
            throw new DirectoryNotFoundException($"The directory '{jobDirectory}' does not exist.");
        }

        JobSummary? jobSummary = null;
        try
        {
            string path = GetFilePath(job.JobId, jobDirectory);
            if (!File.Exists(path))
            {
                logger.LogWarning("Job profile file not found: {Path}", path);
                return job;
            }

            using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);
            {
                jobSummary = await JsonSerializer.DeserializeAsync<JobSummary>(stream, options);
            }

            if (jobSummary != null)
            {
                // Merge properties using reflection
                var properties = typeof(JobSummary).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    // Skip read-only properties
                    if (!property.CanWrite || !property.CanRead)
                        continue;

                    var incomingValue = property.GetValue(job);

                    // Skip if incoming value should be ignored
                    if (ReflectionHelper.ShouldIgnoreProperty(property, incomingValue))
                        continue;

                    // Update the property in the existing job
                    property.SetValue(jobSummary, incomingValue);
                }
            }
        }
        catch (JsonException jsonEx)
        {
            // Log the error and return null if deserialization fails
            logger.LogError(jsonEx, "JSON deserialization error: {Message}", jsonEx.Message);
            // Log the error and return the original job if deserialization fails
            logger.LogError(jsonEx, "JSON deserialization error: {Message}", jsonEx.Message);
            return job;
        }
        catch (Exception ex)
        {
            // Log any other errors that occur
            logger.LogError(ex, "Error reading job profile file: {Message}", ex.Message);
            throw new IOException($"Error reading job profile file: {job.JobId}", ex);
        }

        // Always update the UpdatedAt timestamp if the property exists
        var updatedAtProperty = typeof(JobSummary).GetProperty(nameof(JobSummary.UpdatedAt));
        if (updatedAtProperty != null && updatedAtProperty.CanWrite)
        {
            updatedAtProperty.SetValue(jobSummary, DateTime.UtcNow);
        }
        return jobSummary ?? job;
    }

    private static string GetFilePath(string jobId, string directory) =>
    Path.Combine(directory, $"job.{jobId.FileSystemName()}.json");

    private static string GetJobsDirectory(EzLeadSettings? settings)
    {
        if (settings is not null && !string.IsNullOrEmpty(settings.FileJobProfileDirectory))
        {
            return Path.Combine(settings.FileJobProfileDirectory, "jobs");
        }

        var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (dir is not null && !string.IsNullOrEmpty(dir.ToString()))
        {
            return Path.Combine(dir.ToString(), "cache", "jobs");
        }
        return Path.Combine(AppContext.BaseDirectory, "cache", "jobs");
    }
}
using Application.Models;
using Core.Contracts;
using Core.Extensions;

namespace WebApp.Extensions;

/// <summary>
/// Provides extension methods for various utility operations.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Attempts to retrieve a cached job summary for the specified job ID.
    /// </summary>
    /// <remarks>This method checks the cache for a job summary associated with the given job ID. If the job
    /// summary is found, it is returned and a log entry is created at the information level. If the job summary is not
    /// found, <see langword="null"/> is returned and a log entry is created at the debug level.</remarks>
    /// <param name="cacheService">The cache service used to retrieve the job summary. Cannot be <see langword="null"/>.</param>
    /// <param name="jobId">The unique identifier of the job to retrieve. Must not be <see langword="null"/> or empty.</param>
    /// <param name="logger">The logger used to record diagnostic information. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="JobSummary"/> object representing the cached job summary if found; otherwise, <see
    /// langword="null"/>.</returns>
    public static async Task<JobSummary?> GetJobAsync(
        this ICacheService cacheService,
        string jobId,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(logger);

        var cacheKey = GetCacheKey("Job", jobId);
        var cachedJob = await cacheService.TryGetAsync<JobSummary>(cacheKey);
        if (cachedJob is not null)
        {
            logger.LogInformation("Retrieved job {JobId} from cache", jobId);
            return cachedJob;
        }
        logger.LogDebug("Job {JobId} not found in cache", jobId);
        return null;
    }

    /// <summary>
    /// Attempts to retrieve a list of job summaries from the cache using the specified key.
    /// </summary>
    /// <remarks>This method logs an informational message indicating whether the companies were successfully
    /// retrieved from the cache or if no data was found for the specified key.</remarks>
    /// <param name="cacheService">The cache service used to retrieve the cached data. Cannot be <see langword="null"/>.</param>
    /// <param name="fromDate">The date from which to retrieve job summaries. This is used to create a unique cache key.</param>
    /// <param name="logger">The logger used to record informational messages. Cannot be <see langword="null"/>.</param>
    /// <returns>A collection of <see cref="JobSummary"/> objects if the cache contains data for the specified key; otherwise,
    /// <see langword="null"/>.</returns>
    public static async Task<IEnumerable<JobSummary>?> GetJobsAsync(
        this ICacheService cacheService,
        DateTime fromDate,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(fromDate);
        ArgumentNullException.ThrowIfNull(logger);

        var cacheKey = GetCacheKey("Jobs", fromDate);
        var cachedJobs = await cacheService.TryGetAsync<IEnumerable<JobSummary>>(cacheKey);
        if (cachedJobs is not null)
        {
            logger.LogInformation("Retrieved jobs list from cache");
            return cachedJobs;
        }
        logger.LogDebug("No jobs found in cache for key: {Key}", cacheKey);
        return null;
    }

    /// <summary>
    /// Attempts to add a collection of job summaries to the cache with a specified expiration time.
    /// </summary>
    /// <remarks>The cache key is generated based on the <paramref name="fromDate"/> value, ensuring
    /// uniqueness for each date. This method does not validate the contents of <paramref name="jobs"/>; it simply
    /// stores the provided collection in the cache.</remarks>
    /// <param name="cacheService">The cache service used to store the job summaries. Cannot be <see langword="null"/>.</param>
    /// <param name="jobs">The collection of job summaries to be cached. If <see langword="null"/>, an empty collection will be cached.</param>
    /// <param name="fromDate">The date used to generate the cache key. Cannot be <see langword="null"/>.</param>
    /// <param name="cacheExpirationInMinutes">The duration, in minutes, for which the cache entry will remain valid.</param>
    /// 
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task AddJobsAsync(
    this ICacheService cacheService,
    IEnumerable<JobSummary> jobs,
    DateTime fromDate,
    int cacheExpirationInMinutes)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(fromDate);

        var cacheKey = GetCacheKey("Jobs", fromDate);
        await cacheService.CreateEntryAsync(cacheKey, jobs, TimeSpan.FromMinutes(cacheExpirationInMinutes));
    }


    /// <summary>
    /// Adds a company profile to the cache with a specified expiration time.
    /// </summary>
    /// <param name="cacheService">The cache service used to store the job summaries.</param>
    /// <param name="job"> The job profile to be cached. Cannot be <see langword="null"/>.</param>
    /// <param name="cacheExpirationInMinutes">The duration, in minutes, for which the cache entry remains valid.</param>
    /// 
    /// <returns>A task that represents the asynchronous operation of adding the job summaries to the cache.</returns>
    public static async Task AddJobAsync(
    this ICacheService cacheService,
    JobSummary job,
    int cacheExpirationInMinutes)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(job);

        var cacheKey = GetCacheKey("Job", job.JobId);
        await cacheService.CreateEntryAsync(cacheKey, job, TimeSpan.FromMinutes(cacheExpirationInMinutes));
    }

    /// <summary>
    /// Retrieves a company profile from the cache based on the specified company ID.
    /// </summary>
    /// <remarks>This method attempts to retrieve the company profile from the cache using the provided
    /// <paramref name="companyId"/>. If the profile is found, it logs an informational message. If the profile is not
    /// found, it logs a debug message.</remarks>
    /// <param name="cacheService">The cache service used to retrieve the company profile.</param>
    /// <param name="companyId">The unique identifier of the company whose profile is being retrieved. Cannot be <see langword="null"/>.</param>
    /// <param name="logger">The logger used to record diagnostic information. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="CompanyProfile"/> object representing the company profile if found in the cache; otherwise, <see
    /// langword="null"/>.</returns>
    public static async Task<CompanyProfile?> GetCompanyAsync(
        this ICacheService cacheService,
        string companyId,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(logger);

        var cacheKey = GetCacheKey("Company", companyId);
        var cacheCompany = await cacheService.TryGetAsync<CompanyProfile>(cacheKey);
        if (cacheCompany is not null)
        {
            logger.LogInformation("Retrieved company profile from cache for {companyId}", companyId);
            return cacheCompany;
        }
        logger.LogDebug("Company id {companyId} not found in cache", companyId);
        return null;
    }

    /// <summary>
    /// Retrieves a list of companies from the cache based on the specified date.
    /// </summary>
    /// <remarks>This method attempts to retrieve a list of companies from the cache using a key derived from
    /// the specified date. If the data is found in the cache, it is returned; otherwise, <see langword="null"/> is
    /// returned.</remarks>
    /// <param name="cacheService">The cache service used to retrieve cached data. Cannot be <see langword="null"/>.</param>
    /// <param name="fromDate">The date used to generate the cache key. Cannot be <see langword="null"/>.</param>
    /// <param name="logger">The logger used to log diagnostic information. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="CompanyProfile"/> representing the cached company list,  or <see
    /// langword="null"/> if no data is found in the cache.</returns>
    public static async Task<IEnumerable<CompanyProfile>?> GetCompaniesAsync(
        this ICacheService cacheService,
        DateTime fromDate,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(fromDate);
        ArgumentNullException.ThrowIfNull(logger);

        var cacheKey = GetCacheKey("Companies", fromDate);
        var cacheCompanies = await cacheService.TryGetAsync<IEnumerable<CompanyProfile>>(cacheKey);
        if (cacheCompanies is not null)
        {
            logger.LogInformation("Retrieved company list from cache");
            return cacheCompanies;
        }
        logger.LogDebug("Company list not found in cache for key: {Key}", cacheKey);
        return null;
    }

    /// <summary>
    /// Adds a company profile to the cache with a specified expiration time.
    /// </summary>
    /// <param name="cacheService">The cache service used to store the job summaries.</param>
    /// <param name="company"> The company profile to be cached. Cannot be <see langword="null"/>.</param>
    /// <param name="cacheExpirationInMinutes">The duration, in minutes, for which the cache entry remains valid.</param>
    /// 
    /// <returns>A task that represents the asynchronous operation of adding the job summaries to the cache.</returns>
    public static async Task AddCompanyAsync(
    this ICacheService cacheService,
    CompanyProfile company,
    int cacheExpirationInMinutes)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(company);

        var cacheKey = GetCacheKey("Company", company.CompanyId);
        await cacheService.CreateEntryAsync(cacheKey, company, TimeSpan.FromMinutes(cacheExpirationInMinutes));
    }

    /// <summary>
    /// Adds a collection of job summaries to the cache with a specified expiration time.
    /// </summary>
    /// <remarks>The cache key is generated using the provided <paramref name="fromDate"/> and includes a hash
    /// string and timestamp. Ensure that <paramref name="cacheExpirationInMinutes"/> is a positive value to avoid
    /// unintended behavior.</remarks>
    /// <param name="cacheService">The cache service used to store the job summaries.</param>
    /// <param name="companies">The collection of company profiles to be cached. Can be empty but must not be null.</param>
    /// <param name="fromDate">The date used to generate the cache key. Cannot be null.</param>
    /// <param name="cacheExpirationInMinutes">The duration, in minutes, for which the cache entry remains valid.</param>
    /// 
    /// <returns>A task that represents the asynchronous operation of adding the job summaries to the cache.</returns>
    public static async Task AddCompaniesAsync(
    this ICacheService cacheService,
    IEnumerable<CompanyProfile> companies,
    DateTime fromDate,
    int cacheExpirationInMinutes)
    {
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(fromDate);

        var cacheKey = GetCacheKey("Companies", fromDate);
        await cacheService.CreateEntryAsync(cacheKey, companies, TimeSpan.FromMinutes(cacheExpirationInMinutes));
    }

    /// <summary>
    /// Generates a cache key by combining a specified prefix and a date.
    /// </summary>
    /// <param name="prefix">The prefix to include in the cache key. Cannot be <see langword="null"/>.</param>
    /// <param name="fromDate">The date to include in the cache key. Cannot be <see langword="null"/>.</param>
    /// <returns>A string representing the cache key, formatted as "<paramref name="prefix"/>_<paramref name="fromDate"/>".</returns>
    public static string GetCacheKey(string prefix, DateTime fromDate)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(fromDate);
        return $"{prefix}_{fromDate.Date.GenHashString()}";
    }

    /// <summary>
    /// Generates a cache key by combining a specified prefix and a date.
    /// </summary>
    /// <param name="prefix">The prefix to include in the cache key. Cannot be <see langword="null"/>.</param>
    /// <param name="key">The key to include in the cache key. Cannot be <see langword="null"/>.</param>
    /// <returns>A string representing the cache key.</returns>
    public static string GetCacheKey(string prefix, string key)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        ArgumentNullException.ThrowIfNull(key);
        return $"{prefix}_{key.GenHashString()}";
    }
}

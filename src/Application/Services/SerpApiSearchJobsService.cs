using Application.Contracts;
using Application.Models;
using Core.Configuration;
using Core.Contracts;
using Core.Extensions;
using Core.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Application.Services;

/// <summary>
/// Provides functionality to retrieve job listings using the SerpApi service.
/// </summary>
/// <remarks>This service is configured with API settings and utilizes an HTTP client factory to perform requests.
/// It requires valid API key, base address, and endpoint settings to function correctly.</remarks>
sealed public class SerpApiSearchJobsService : IJobsRetrieval<JobResult>
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SerpApiSearchJobsService> _logger;
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Gets the cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationInMinutes { get; }

    /// <summary>
    /// Gets the API key used for authenticating requests.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Gets the name of the client.
    /// </summary>
    public string ClientName { get; }

    /// <summary>
    /// Gets the API endpoint URL for search operations.
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerpApiSearchJobsService"/> class with the specified configuration
    /// settings and HTTP client factory.
    /// </summary>
    /// <param name="cacheService">The cache service used for caching job listings. Cannot be null.</param>
    /// <param name="clientFactory">The factory used to create HTTP client instances. Cannot be null.</param>
    /// <param name="options">The configuration settings for the service, including API key, base address, and endpoint. Cannot be null, and
    /// must contain valid values for <c>ApiKey</c>, <c>BaseAddress</c>, and <c>Endpoint</c>.</param>
    /// <param name="logger">The logger used for logging service operations. Cannot be null.</param>
    public SerpApiSearchJobsService(
        ICacheService cacheService,
        IHttpClientFactory clientFactory,
        IOptions<SerpApiSettings> options,
        ILogger<SerpApiSearchJobsService> logger)
    {
        ArgumentNullException.ThrowIfNull(clientFactory, nameof(clientFactory));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(options.Value.ApiKey, nameof(options.Value.ApiKey));
        ArgumentNullException.ThrowIfNull(options.Value.Endpoint, nameof(options.Value.Endpoint));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
        _clientFactory = clientFactory;
        _cacheService = cacheService;
        CacheExpirationInMinutes = options.Value.CacheExpirationInMinutes;
        ApiKey = options.Value.ApiKey;
        ClientName = options.Value.HttpClientName;
        Endpoint = options.Value.Endpoint;
    }

    /// <summary>
    /// Asynchronously fetches job listings based on the specified query and location.
    /// </summary>
    /// <param name="query">The search term used to find relevant job listings.</param>
    /// <param name="location">The geographical location to filter job listings.</param>
    /// <returns>A task representing the asynchronous operation, with a string containing the job listings in JSON format.</returns>
    public async Task<IEnumerable<JobResult>> FetchJobs(string query, string location)
    {
        var cacheKey = CachingHelper.GenCacheKey(query, location.GenHashString());

        var cachedJobs = await GetCachedJobs(cacheKey);
        if (cachedJobs is not null)
        {
            return cachedJobs.JobsResults;
        }

        try
        {
            var request = $"{Endpoint}?engine=google_jobs&q={Uri.EscapeDataString(query)}&location={Uri.EscapeDataString(location)}&api_key={ApiKey}&hl=en";
            var googleJobs = await GoogleJobsResultAsync(request, location);

            if (googleJobs is null || googleJobs.JobsResults is null || googleJobs.JobsResults.Count == 0)
            {
                _logger.LogWarning("Deserialized job listings are empty for query: {Query} and location: {Location}", query, location);
                return [];
            }

            if (!string.IsNullOrEmpty(googleJobs.SerpapiPagination.NextPageToken))
            {
                var nextPage = await GoogleJobsResultAsync($"{request}&next_page_token={googleJobs.SerpapiPagination.NextPageToken}", location);
                if (nextPage is not null && nextPage.JobsResults is not null && nextPage.JobsResults.Count > 0)
                {
                    googleJobs.JobsResults.AddRange(nextPage.JobsResults);
                }
            }

            await _cacheService.CreateEntryAsync(cacheKey, googleJobs, TimeSpan.FromMinutes(CacheExpirationInMinutes));
            return googleJobs.JobsResults;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing job listings for query: {Query} and location: {Location}", query, location);
        }
        return [];
    }

    private async Task<GoogleJobsResult?> GoogleJobsResultAsync(string request, string location)
    {
        try
        {
            var httpClient = _clientFactory.CreateClient(ClientName);
            var response = await httpClient.GetAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve job listings for query: {request}, Status: {StatusCode}", request, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("No job listings found for query: {Query} and location: {Location}", request, location);
                return null;
            }
            return JsonSerializer.Deserialize<GoogleJobsResult>(content, _options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing job listings for request: {Request} and location: {Location}", request, location);
            return null;
        }
    }

    private async Task<GoogleJobsResult?> GetCachedJobs(string cacheKey)
    {
        var cachedJobs = await _cacheService.TryGetAsync<GoogleJobsResult>(cacheKey);
        if (cachedJobs is not null)
        {
            _logger.LogInformation("Returning cached GoogleJobsResult for key: {CacheKey}", cacheKey);
            return cachedJobs;
        }
        return null;
    }
}


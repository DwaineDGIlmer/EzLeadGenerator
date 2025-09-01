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
/// Provides functionality to perform search operations using the Serp API, specifically targeting the Google search
/// engine.
/// </summary>
/// <remarks>This service handles the execution of search queries, caching of results, and logging of operations.
/// It requires configuration settings for the API key, endpoint, and cache expiration, as well as services for caching,
/// HTTP client creation, and logging.</remarks>
public sealed class SerpApiSearchService : ISearch
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SerpApiSearchService> _logger;
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
    /// Initializes a new instance of the <see cref="SerpApiSearchService"/> class with the specified configuration
    /// settings, cache service, HTTP client factory, and logger.
    /// </summary>
    /// <param name="options">The configuration settings for the Serp API, including the API key, endpoint, and cache expiration settings.
    /// Cannot be null, and must contain valid API key and endpoint values.</param>
    /// <param name="cacheService">The service used for caching search results. Cannot be null.</param>
    /// <param name="clientFactory">The factory used to create HTTP clients for making requests to the Serp API. Cannot be null.</param>
    /// <param name="logger">The logger used for logging information and errors. Cannot be null.</param>
    public SerpApiSearchService(
        IOptions<SerpApiSettings> options,
        ICacheService cacheService,
        IHttpClientFactory clientFactory,
        ILogger<SerpApiSearchService> logger)
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
    /// Fetches search results from the Google Search API based on the specified query and location.
    /// </summary>
    /// <remarks>This method attempts to retrieve cached results before making a request to the Google Search
    /// API.  If the results are not cached, it sends an HTTP request to the API, deserializes the response, and caches
    /// the result for future use.</remarks>
    /// <typeparam name="T">The type of the search result to return. Must be compatible with the deserialized result.</typeparam>
    /// <param name="query">The search query string. Cannot be null or empty.</param>
    /// <param name="location">The location to use for the search. Defaults to "United States" if not specified.</param>
    /// <returns>An asynchronous task that returns an <see cref="IEnumerable{T}"/> containing the search results.  Returns an
    /// empty collection if no results are found or if an error occurs. Returns <see langword="null"/> if the result
    /// type is incompatible.</returns>
    public async Task<IEnumerable<T>?> FetchSearchResults<T>(string query, string location = "United States")
    {
        var cacheKey = CachingHelper.GenCacheKey($"{nameof(SerpApiSearchService)}_{nameof(FetchSearchResults)}", query, location.GenHashString());

        // Try to get cached search first
        var cachedSearch = await GetCachedSearchResults<GoogleSearchResult>(cacheKey);
        if (cachedSearch is not null && cachedSearch is GoogleSearchResult googleSearchResult)
        {
            return [(T)(object)googleSearchResult];
        }

        var request = $"{Endpoint}?engine=google&q={Uri.EscapeDataString(query)}&location={Uri.EscapeDataString(location)}&api_key={ApiKey}&hl=en";
        var httpClient = _clientFactory.CreateClient(ClientName);
        var response = await httpClient.GetAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve results for query: {Query} and location: {Location}. Status: {StatusCode}", query, location, response.StatusCode);
            return [];
        }

        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            _logger.LogInformation("No results found for query: {Query} and location: {Location}", query, location);
            return [];
        }

        try
        {
            var googleSearch = JsonSerializer.Deserialize<GoogleSearchResult>(content, _options);
            if (googleSearch is null)
            {
                _logger.LogWarning("Deserialized search result for query: {Query} and location: {Location}", query, location);
                return [];
            }

            await _cacheService.CreateEntryAsync(cacheKey, googleSearch, TimeSpan.FromMinutes(CacheExpirationInMinutes));
            return [(T)(object)googleSearch];
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing search result for query: {Query} and location: {Location}", query, location);
            return [];
        }
    }

    /// <summary>
    /// Performs an asynchronous search using the Google search engine for the specified query and location.
    /// </summary>
    /// <remarks>The method first attempts to retrieve cached search results. If no cached results are
    /// available, it performs a new search request to the Google search engine. The results are cached for future
    /// requests. Logs errors and warnings if the search fails or returns no results.</remarks>
    /// <param name="query">The search query string. Cannot be null or empty.</param>
    /// <param name="location">The geographical location to refine the search results. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GoogleSearchResult"/>
    /// object with the search results, or <see langword="null"/> if no results are found or if an error occurs.</returns>
    public async Task<IEnumerable<T>?> FetchOrganicResults<T>(string query, string location = "United States")
    {
        var googleSearch = await FetchSearchResults<GoogleSearchResult>(query, location);
        if (googleSearch is null || !googleSearch.Any())
        {
            _logger.LogWarning("FetchOrganicResults failed to find results for query: {Query} and location: {Location}", query, location);
            return [];
        }

        // If the type is not OrganicResult, return the first result's OrganicResults property
        var result = googleSearch?.FirstOrDefault()?.OrganicResults ?? [];
        if (typeof(T) != typeof(OrganicResult))
        {
            return googleSearch as IEnumerable<T>;
        }

        // Otherwise, cast the results to the specified type and in our case , it should be OrganicResult
        var castedResults = result
            .Where(r => r is T)
            .Cast<T>()
            .ToList();

        if (castedResults.Count != 0)
        {
            return castedResults;
        }

        _logger.LogWarning("Result type does not contain OrganicResults property for query: {Query} and location: {Location}", query, location);
        return [];
    }

    private async Task<T?> GetCachedSearchResults<T>(string cacheKey)
    {
        var cachedSearch = await _cacheService.TryGetAsync<T>(cacheKey);
        if (cachedSearch is not null)
        {
            _logger.LogInformation("Returning cached GoogleSearch Result for key: {CacheKey}", cacheKey);
            return cachedSearch;
        }
        return default;
    }
}

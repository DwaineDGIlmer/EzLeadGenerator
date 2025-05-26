using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApp.Constants;
using WebApp.Extensions;

namespace WebApp.Pages;


/// <summary>
/// The page model for the Index page, handling search queries and lead generation logic.
/// Initializes a new instance of the <see cref="IndexModel"/> class with the specified dependencies.
/// </summary>
/// <param name="config">The application configuration.</param>
/// <param name="cache">The memory cache instance.</param>
/// <param name="httpClientFactory">The HTTP client factory to create resilent client.</param>
/// <param name="logger">The logger instance for this model.</param>
public partial class IndexModel(IConfiguration config, IMemoryCache cache, IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger) : PageModel
{
    private readonly IConfiguration _config = config;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<IndexModel> _logger = logger;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(nameof(EzLeadGenerator));

    /// <summary>
    /// Gets or sets the search query entered by the user.
    /// </summary>
    [BindProperty]
    public string SearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the search query should be treated as an exact phrase.
    /// </summary>
    [BindProperty]
    public bool SearchAsPhrase { get; set; } = false;

    /// <summary>
    /// Gets or sets the current page number for paginated data retrieval.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the list of selected platform URLs.
    /// </summary>
    [BindProperty]
    public List<string> SelectedPlatforms { get; set; } = ["linkedin.com/in", "github.com", "youtube.com"];

    /// <summary>
    /// Gets or sets the list of selected domain names.
    /// </summary>
    [BindProperty]
    public List<string> SelectedDomains { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether only records with a phone number should be exported.
    /// </summary>
    [BindProperty]
    public bool ExportOnlyWithPhone { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the query is considered too short.
    /// </summary>
    [BindProperty]
    public bool QueryTooShort { get; set; } = false;

    /// <summary>
    /// Gets or sets the collection of lead information results.
    /// </summary>
    public List<LeadInfo> Results { get; set; } = [];

    /// <summary>
    /// Handles the HTTP POST request for the page.
    /// </summary>
    /// <remarks>This method processes the POST request by performing necessary operations  and then returns
    /// the current page. It is typically used to handle form submissions  or other POST-based interactions on the
    /// page.</remarks>
    /// <returns>An <see cref="IActionResult"/> that represents the result of the operation.  Typically, this is the current
    /// page.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await FetchLeadsAsync();
        return Page();
    }

    /// <summary>
    /// Handles the HTTP POST request to generate and download a CSV file containing lead data.
    /// </summary>
    /// <remarks>The method fetches lead data, filters it based on the <see cref="ExportOnlyWithPhone"/>
    /// property,  and generates a CSV file with the fields "Name", "Email", "Phone", and "Link". The resulting file  is
    /// returned as a downloadable response with the appropriate MIME type and file name.</remarks>
    /// <returns>An <see cref="IActionResult"/> that represents the downloadable CSV file containing the lead data.</returns>
    public async Task<IActionResult> OnPostDownloadAsync()
    {
        await FetchLeadsAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Name,Email,Phone,Link");

        var exportResults = ExportOnlyWithPhone ? Results.Where(r => !string.IsNullOrWhiteSpace(r.Phone)) : Results;

        foreach (var lead in exportResults)
        {
            csv.AppendLine($"\"{lead.Name}\",\"{lead.Email}\",\"{lead.Phone}\",\"{lead.Link}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, Defaults.CsvMimeType, Defaults.LeadFileName);
    }

    /// <summary>
    /// Asynchronously fetches lead information based on the current search query, selected platforms, and optional
    /// domain filters.
    /// </summary>
    /// <remarks>This method performs a search using the configured API endpoint and retrieves lead
    /// information such as names, emails, and phone numbers. Results are cached to improve performance for repeated
    /// queries with the same parameters.</remarks>
    /// <returns></returns>
    private async Task FetchLeadsAsync()
    {
        Results.Clear();
        var apiKey = _config[Defaults.GetSettings(Defaults.SearchApiKey)] ?? Environment.GetEnvironmentVariable(Defaults.EnvSearchApiKey);
        apiKey.IsNullThrow();

        var baseEndpoint = _config[Defaults.GetSettings(Defaults.SearchEndpoint)] ?? Environment.GetEnvironmentVariable(Defaults.EnvSearchApiUrl);
        baseEndpoint.IsNullThrow();

        if (string.IsNullOrWhiteSpace(SearchQuery) || SearchQuery.Length <= 3)
        {
            QueryTooShort = true;
            return;
        }
        QueryTooShort = false;

        if (SelectedPlatforms == null || SelectedPlatforms.Count == 0)
            return;

        var platforms = SelectedPlatforms;
        var domains = SelectedDomains?.Where(d => !string.IsNullOrWhiteSpace(d)).ToList() ?? [];

        var cacheKey = $"leads:{SearchQuery}:{string.Join(",", platforms)}:{string.Join(",", domains)}:{PageNumber}";
        if (_cache.TryGetValue(cacheKey, out List<LeadInfo>? cachedResults))
        {
            Results.AddRange(cachedResults!);
            return;
        }

        var domainFilter = domains.Count != 0
            ? $"({string.Join(" OR ", domains.Select(d => $"\"{d}\""))})"
            : string.Empty;

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Defaults.JsonMimeType));
        foreach (var platform in platforms)
        {
            var query = SearchAsPhrase
                 ? $"site:{platform} \"{SearchQuery}\""
                 : $"site:{platform} {SearchQuery}";

            var endpoint = $"{baseEndpoint}?engine=google&q={Uri.EscapeDataString(query)}&api_key={apiKey}&start={(PageNumber - 1) * 10}";

            _logger.LogInformation("Fetching leads for query: {SearchQuery}, Platforms: {Platforms}, Domains: {Domains}, Page: {PageNumber}", SearchQuery, platforms, domainFilter, PageNumber);
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch job results: {StatusCode}", response.StatusCode);
                continue;
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            if (!json.RootElement.TryGetProperty("organic_results", out var results)) continue;

            foreach (var result in results.EnumerateArray())
            {
                var url = result.TryGetProperty("link", out var linkProp) && linkProp.ValueKind == JsonValueKind.String
                    ? linkProp.GetString()
                    : (result.TryGetProperty("url", out var urlProp) && urlProp.ValueKind == JsonValueKind.String
                        ? urlProp.GetString()
                        : string.Empty);

                var snippet = result.TryGetProperty("snippet", out var snippetProp) && snippetProp.ValueKind == JsonValueKind.String
                    ? snippetProp.GetString()
                    : (result.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String
                        ? descProp.GetString()
                        : string.Empty);

                var rawTitle = result.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String
                    ? titleProp.GetString()
                    : (result.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                        ? nameProp.GetString()
                        : string.Empty);

                var name = WebUtility.HtmlDecode(rawTitle ?? string.Empty);

                if (string.IsNullOrWhiteSpace(snippet))
                    continue;

                var emails = EmailRegex().Matches(snippet)
                                .Cast<Match>()
                                .Select(m => m.Value)
                                .Where(email => domains.Count == 0 || domains.Any(d => email.EndsWith(d)))
                                .Distinct();

                var phones = PhoneRegex().Matches(snippet)
                                .Cast<Match>()
                                .Select(m => m.Value)
                                .Distinct();

                foreach (var email in emails)
                {
                    Results.Add(new LeadInfo
                    {
                        Name = name,
                        Email = email,
                        Phone = phones.FirstOrDefault() ?? string.Empty,
                        Link = url ?? string.Empty,
                        Platform = platform
                    });
                }
            }
        }
        _cache.Set(cacheKey, Results.ToList(), TimeSpan.FromMinutes(10));
    }

    [GeneratedRegex(@"[\w\.-]+@[\w\.-]+\.[a-z]{2,}")]
    private static partial Regex EmailRegex();
    [GeneratedRegex(@"\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}")]
    private static partial Regex PhoneRegex();
}

/// <summary>
/// Represents information about a lead, including contact details and platform-specific data.
/// </summary>
/// <remarks>This class is typically used to store and transfer information about a lead, such as their name, 
/// email, phone number, and the platform or source from which the lead originated.  It can be used in customer
/// relationship management (CRM) systems or similar applications.</remarks>
public class LeadInfo
{
    /// <summary>
    /// Gets or sets the name associated with the object.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address associated with the entity.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number associated with the entity.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hyperlink associated with the current object.
    /// </summary>
    public string Link { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the platform name associated with the current context.
    /// </summary>
    public string Platform { get; set; } = string.Empty;
}

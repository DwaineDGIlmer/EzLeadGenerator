using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebApp.Constants;
using WebApp.Extensions;

namespace WebApp.Pages;

/// <summary>
/// Represents the model for managing talent demand, including job search functionality and result grouping.
/// </summary>
/// <remarks>This model is used to handle job search queries, process results from an external API, and provide
/// functionality for downloading job listings in CSV format. It includes properties for specifying search criteria such
/// as job title and location, as well as options for customizing the search behavior.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="TalentDemandModel"/> class with the specified configuration.
/// </remarks>
/// <param name="config">The configuration settings used by the model.</param>
/// <param name="cache">The memory cache instance used for caching data.</param>
/// <param name="httpClientFactory">The HTTP client factory to creat resilent client.</param>
/// <param name="logger">The logger instance for this model.</param>
public class TalentDemandModel(IConfiguration config, IMemoryCache cache, IHttpClientFactory httpClientFactory, ILogger<TalentDemandModel> logger) : PageModel
{
    private readonly IConfiguration _config = config;
    private readonly IMemoryCache _cache = cache;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(nameof(EzLeadGenerator));
    private readonly ILogger<TalentDemandModel> _logger = logger;

    /// <summary>
    /// Gets or sets the job title associated with the current entity.
    /// </summary>
    [BindProperty]
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location associated with the current context.
    /// </summary>
    [BindProperty]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    ///     
    /// </summary>
    [BindProperty]
    public bool IncludeDescriptions { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the query is considered too short.
    /// </summary>
    [BindProperty]
    public bool QueryTooShort { get; set; } = false;

    /// <summary>
    /// Gets or sets the key used to identify cached results.
    /// </summary>
    [BindProperty]
    public string ResultsCacheKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to search for the job title as a phrase. 
    /// </summary>
    [BindProperty]
    public bool SearchJobTitleAsPhrase { get; set; } = false;

    /// <summary>
    /// /// Gets or sets a value indicating whether to search for the job title as a phrase.
    /// </summary>
    public Dictionary<string, List<JobPosting>> GroupedJobResults { get; set; } = [];

    /// <summary>
    /// Handles the HTTP POST request for the page.
    /// </summary>
    /// <remarks>This method processes job results asynchronously, stores the results in temporary data for
    /// later use, and returns the current page. The job results are serialized as JSON and stored in the 
    /// ITempDictionary under the key "LastJobResults".</remarks>
    /// <returns>An <see cref="IActionResult"/> that renders the current page.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        await FetchJobResultsAsync();
        return Page();
    }

    /// <summary>
    /// Handles the HTTP POST request to generate and download a CSV file containing job listings.
    /// </summary>
    /// <remarks>This method retrieves job results from temporary data or fetches them if not available, 
    /// formats the results into a CSV file, and returns the file as a downloadable response.</remarks>
    /// <returns>An <see cref="IActionResult"/> that represents the CSV file containing job listings. The file is returned with a
    /// MIME type of "text/csv" and a default filename of "job_listings.csv".</returns>
    public async Task<IActionResult> OnPostDownloadAsync()
    {
        await Task.Delay(100);

        if (_cache.TryGetValue(ResultsCacheKey, out Dictionary<string, List<JobPosting>>? cachedResults) &&
           cachedResults.IsNotNull() &&
           cachedResults!.Count > 0)
        {
            GroupedJobResults = cachedResults;
        }
        else
        {
            // Handle cache miss
            return RedirectToPage();
        }

        var csv = new StringBuilder();
        csv.AppendLine("Title,Company,Location,Type,Posted,Description,ApplyLink");

        foreach (var group in GroupedJobResults)
        {
            foreach (var job in group.Value)
            {
                // Escape quotes and commas in fields
                static string Clean(string s) => "\"" + (s ?? string.Empty).Replace("\"", "\"\"") + "\"";
                csv.AppendLine($"{Clean(job.Title)},{Clean(job.Company)},{Clean(job.Location)},{Clean(job.Type)},{Clean(job.Posted)},{Clean(job.Description)},{Clean(job.ApplyLink)}");
            }
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", "job_listings.csv");
    }

    /// <summary>
    /// Fetches job postings from an external job search API based on the specified job title and location.
    /// </summary>
    /// <remarks>This method constructs a query using the provided job title and location, sends a request to
    /// the job search API,  and processes the response to extract job postings. The results are grouped by company and
    /// stored in the  <see cref="GroupedJobResults"/> property. If the query is too short, the method sets <see
    /// cref="QueryTooShort"/>  to <see langword="true"/> and exits without making an API call.</remarks>
    /// <returns></returns>
    private async Task FetchJobResultsAsync()
    {
        if (string.IsNullOrWhiteSpace(JobTitle) ||
            string.IsNullOrEmpty(Location) ||
            JobTitle.Length <= 3 ||
            Location.Length <= 3)
        {
            QueryTooShort = true;
            return;
        }
        QueryTooShort = false;

        ResultsCacheKey = $"talent:{JobTitle}:{string.Join(",", Location)}:{string.Join(",", SearchJobTitleAsPhrase)}:{IncludeDescriptions}";
        if (_cache.TryGetValue(ResultsCacheKey, out Dictionary<string, List<JobPosting>>? cachedResults) &&
            cachedResults.IsNotNull() &&
            cachedResults!.Count > 0)
        {
            GroupedJobResults = cachedResults!;
            return;
        }

        var jobResults = new List<JobPosting>();
        var apiKey = _config[Defaults.GetSettings(Defaults.SearchApiKey)] ?? Environment.GetEnvironmentVariable(Defaults.EnvSearchApiKey);
        apiKey.IsNullThrow();

        var baseEndpoint = _config[Defaults.GetSettings(Defaults.SearchEndpoint)] ?? Environment.GetEnvironmentVariable(Defaults.EnvSearchApiUrl);
        baseEndpoint.IsNullThrow();

        var value = _config[Defaults.GetSettings(Defaults.LogToFileSystem)];
        bool logToFileSystem = false;
        if (!string.IsNullOrEmpty(value))
        {
            _ = bool.TryParse(value, out logToFileSystem);
        }

        string query = SearchJobTitleAsPhrase
            ? $"\"{JobTitle}\" \"{Location}\""
            : $"{JobTitle} \"{Location}\"";
        string url = $"{baseEndpoint}?engine=google_jobs&q={Uri.EscapeDataString(query)}&api_key={apiKey}";

        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Defaults.JsonMimeType));
        _logger.LogInformation("Fetching job results from {Url}", url);

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch lead results: {StatusCode}", response.StatusCode);
            return;
        }
        _logger.LogInformation("Success in fetching lead results: {StatusCode}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        if (content.IsNullOrEmpty())
        {
            _logger.LogWarning("Received empty content for query: {SearchQuery}", query);
            return;
        }

        if (logToFileSystem)
        {
            var filename = Path.Combine(Path.GetTempPath(), $"Talent.{GenerateRandomString()}.json");
            await System.IO.File.WriteAllTextAsync(filename, content);
        }

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        if (!json.RootElement.TryGetProperty("jobs_results", out var jobs) || jobs.ValueKind != JsonValueKind.Array) return;

        foreach (var job in jobs.EnumerateArray())
        {
            string title = job.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String
                ? titleProp.GetString() ?? string.Empty
                : string.Empty;

            string company = job.TryGetProperty("company_name", out var companyProp) && companyProp.ValueKind == JsonValueKind.String
                ? companyProp.GetString() ?? string.Empty
                : string.Empty;

            string location = job.TryGetProperty("location", out var locationProp) && locationProp.ValueKind == JsonValueKind.String
                ? locationProp.GetString() ?? string.Empty
                : string.Empty;

            string type = string.Empty;
            if (job.TryGetProperty("extensions", out var extProp) && extProp.ValueKind == JsonValueKind.Array && extProp.GetArrayLength() > 0)
            {
                type = string.Join(", ", extProp.EnumerateArray()
                    .Where(e => e.ValueKind == JsonValueKind.String)
                    .Select(e => e.GetString()));
            }

            string posted = string.Empty;
            if (job.TryGetProperty("detected_extensions", out var postedExt) &&
                postedExt.ValueKind == JsonValueKind.Object &&
                postedExt.TryGetProperty("posted_at", out var postedAt) &&
                postedAt.ValueKind == JsonValueKind.String)
            {
                posted = postedAt.GetString() ?? string.Empty;
            }

            string description = job.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String
                ? descProp.GetString() ?? string.Empty
                : string.Empty;

            string applyLink = string.Empty;
            if (job.TryGetProperty("apply_options", out var applyOpts) && applyOpts.ValueKind == JsonValueKind.Array && applyOpts.GetArrayLength() > 0)
            {
                var firstOpt = applyOpts[0];
                if (firstOpt.ValueKind == JsonValueKind.Object &&
                    firstOpt.TryGetProperty("link", out var linkProp) &&
                    linkProp.ValueKind == JsonValueKind.String)
                {
                    applyLink = linkProp.GetString() ?? string.Empty;
                }
            }

            jobResults.Add(new JobPosting
            {
                Title = title,
                Company = company,
                Location = location,
                Type = type,
                Posted = posted,
                Description = description,
                ApplyLink = applyLink
            });
        }

        GroupedJobResults = jobResults
            .GroupBy(j => string.IsNullOrWhiteSpace(j.Company) ? "Unknown Company" : j.Company)
            .ToDictionary(g => g.Key, g => g.ToList());

        _cache.Set(ResultsCacheKey, GroupedJobResults, TimeSpan.FromMinutes(20));
    }

    /// <summary>
    /// Generates a random alphanumeric string of the specified length.
    /// </summary>
    /// <remarks>The generated string is composed of characters from the set:  'A-Z', 'a-z', and '0-9'. A new
    /// instance of <see cref="Random"/> is used for each call,  which may result in less randomness if called in quick
    /// succession.</remarks>
    /// <param name="length">The length of the random string to generate. The default value is 10. Must be a non-negative integer.</param>
    /// <returns>A randomly generated string consisting of uppercase letters, lowercase letters, and digits.</returns>
    public static string GenerateRandomString(int length = 10)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
    }
}

/// <summary>
/// Represents a job posting with details such as title, company, location, type, posting date, description, and
/// application link.
/// </summary>
/// <remarks>This class is designed to encapsulate the essential information about a job posting.  It can be used
/// to display job listings, store job-related data, or facilitate job application workflows.</remarks>
public class JobPosting
{
    /// <summary>
    /// Gets or sets the title associated with the current object.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the company.
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location associated with the current instance.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the entity or object represented by this instance.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time the content was posted, represented as a string.
    /// </summary>
    public string Posted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description associated with the object.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL link to apply for the associated resource or opportunity.
    /// </summary>
    public string ApplyLink { get; set; } = string.Empty;
}

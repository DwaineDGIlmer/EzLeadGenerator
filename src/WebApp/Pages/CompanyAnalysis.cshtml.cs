using Application.Contracts;
using Application.Models;

namespace WebApp.Pages;

/// <summary>
/// Represents a page model for analyzing company data, providing functionality to retrieve and display company
/// summaries.
/// </summary>
/// <remarks>This class is designed to interact with a company repository to fetch and display a paginated
/// list of company profiles. It initializes with a specified repository and provides an asynchronous method to load
/// the first page of company summaries.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="IndexModel"/> class with the specified company repository.
/// </remarks>
/// <param name="repository">The repository used to access company data. Cannot be null.</param>
/// <param name="search">The search service used to perform searches for company-related data. Cannot be null.</param>
/// <param name="logger">The logger used for logging information and errors. Cannot be null.</param>
sealed public class CompanyAnalysisPage(
    IDisplayRepository repository,
    ISearch search,
    ILogger<CompanyAnalysisPage> logger) : PageModel
{
    private readonly static List<string> _jobKeywords = ["pay", "career", "careers", "job", "jobs", "position", "employment", "work", "vacancy", "opening", "opportunity", "hiring"];
    private readonly static List<string> _newsKeywords = ["innovation", "win", "partner", "strategic", "news", "update", "announcement", "press release", "report", "article", "story", "coverage", "media"];
    private readonly static List<string> _programKeywords = ["innovations", "compliant", "education", "invest", "design", "government", "collaborate", "support ", "program", "initiative", "project", "campaign", "strategy", "plan", "effort", "action", "activity"];
    private readonly static char[] _spltOptions = [' ', ',', '.', ';', ':', '-', '!', '?', '/', '\\'];

    private readonly IDisplayRepository _displayRepository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ISearch _search = search ?? throw new ArgumentNullException(nameof(search));
    private readonly ILogger<CompanyAnalysisPage> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Gets the current page number in a paginated list or document.
    /// </summary>
    public int PageNumber { get; private set; } = 1;

    /// <summary>
    /// Gets the number of items to display per page.
    /// </summary>
    public int PageSize { get; private set; } = 1;

    /// <summary>
    /// Gets the total count of items processed.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// The company results displayed on the page, each containing a list of items.
    /// </summary>
    public List<SectionVm> Sections { get; set; } = [];

    /// <summary>
    /// Gets the collection of job summaries.
    /// </summary>
    public List<CompanyProfile> CompanySummaries { get; private set; } = [];

    /// <summary>
    /// Gets or sets the collection of related search results.
    /// </summary>
    public List<RelatedSearch> RelatedSearches { get; set; } = [];

    /// <summary>
    /// Asynchronously retrieves a paginated list of company summaries.
    /// </summary>
    /// <remarks>This method fetches the first page of company summaries with a fixed page size of
    /// 25.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task OnGetAsync([FromQuery] int page = 1)
    {
        TotalCount = _displayRepository.GetJobCount(DateTime.Now.AddDays(-30));
        PageNumber = page < 1 ? 1 : page;
        CompanySummaries = [.. _displayRepository.GetPaginatedCompanies(DateTime.Now.AddDays(-30), PageNumber, PageSize)];
        if (CompanySummaries == null || CompanySummaries.Count == 0)
        {
            _logger.LogWarning("No company summaries found.");
            return;
        }

        var result = _displayRepository.GetPaginatedCompanies(DateTime.Now.AddDays(-30), PageNumber, 1).FirstOrDefault();
        if (result == null)
        {
            _logger.LogWarning("No company summaries found for page {PageNumber}.", PageNumber);
            return;
        }

        await GetCompanyResults(result);
    }

    /// <summary>
    /// Retrieves a list of organic search results related to the specified company's profile, focusing on data
    /// analytics and services.
    /// </summary>
    /// <remarks>The method uses the provided search service to query for recent information about the
    /// company,  focusing on topics related to data analytics and services. The results are limited to the top 10 items
    /// and include details such as title, URL, source, and snippet.</remarks>
    /// <param name="companyProfile">The profile of the company for which search results are to be retrieved. Must not be null.</param>
    /// 
    /// <returns>A list of <see cref="OrganicResultItem"/> objects representing the top 10 relevant search results.  Returns an
    /// empty list if no results are found.</returns>
    public async Task GetCompanyResults(CompanyProfile companyProfile)
    {
        List<OrganicResultItem> resultItemVms = [];
        var searchResults = await _search.FetchSearchResults<GoogleSearchResult>($"All recent information on the company \"{companyProfile.CompanyName}\", focusing on Data analytics and services.", "NC");
        if (searchResults is null || !searchResults.Any())
        {
            return;
        }

        var searchResult = searchResults?.FirstOrDefault();
        RelatedSearches = searchResult?.RelatedSearches ?? [];

        var result = searchResult?.OrganicResults;
        if (result is null)
        {
            return;
        }

        Sections.Clear();
        foreach (var item in result.Take(10))
        {
            resultItemVms.Add(new OrganicResultItem
            {
                Title = item.Title,
                Url = item.Link,
                Source = item.Source,
                DisplayedLink = item.DisplayedLink,
                Date = item.Date,
                MustInclude = item.MustInclude,
                Snippet = item.Snippet,
                Type = GetTitle(item.Snippet, item.Link.ToString()),
                Tags = GetTags(companyProfile.CompanyName, item.SnippetHighlightedWords, item.Snippet, item.Link.ToString())
            });
        }

        Sections = [.. resultItemVms
            .OrderBy(item => item.Date)
            .GroupBy(item => item.Type)
            .Select(group => new SectionVm
            {
                Title = group.Key,
                Items = [.. group]
            })];
        _logger.LogInformation("Retrieved {Count} company summaries for page {PageNumber}.", CompanySummaries.Count, PageNumber);
    }

    /// <summary>
    /// Determines the title category based on the provided snippet and display link.
    /// </summary>
    /// <remarks>The method uses predefined keyword lists to determine the category. If no keywords match, 
    /// the default category <see langword="buisness"/> is returned.</remarks>
    /// <param name="snippet">A text snippet to analyze for keywords.</param>
    /// <param name="displayLink">A display link to analyze for keywords.</param>
    /// <returns>A string representing the determined title category. Possible values are: <list type="bullet">
    /// <item><description><see langword="News"/> if the snippet or display link contains news-related
    /// keywords.</description></item> <item><description><see langword="Program"/> if the snippet or display link
    /// contains program-related keywords.</description></item> <item><description><see langword="Job"/> if the snippet
    /// or display link contains job-related keywords.</description></item> <item><description><see
    /// langword="buisness"/> if no specific category is matched.</description></item> </list></returns>
    public static string GetTitle(string snippet, string displayLink)
    {
        if (!string.IsNullOrWhiteSpace(snippet))
        {
            var words = snippet.Split(_spltOptions, StringSplitOptions.RemoveEmptyEntries)
                               .Select(w => w.ToLowerInvariant())
                               .ToList();
            var displayWords = displayLink.Split(_spltOptions, StringSplitOptions.RemoveEmptyEntries)
                                          .Select(w => w.ToLowerInvariant())
                                          .ToList();

            if (_newsKeywords.Any(word => words.Contains(word.ToLowerInvariant())) ||
                _newsKeywords.Any(word => displayWords.Contains(word.ToLowerInvariant())))
            {
                return "News";
            }
            if (_programKeywords.Any(word => words.Contains(word.ToLowerInvariant())) ||
                _programKeywords.Any(word => displayWords.Contains(word.ToLowerInvariant())))
            {
                return "Program";
            }
            if (_jobKeywords.Any(word => words.Contains(word.ToLowerInvariant())) ||
                _jobKeywords.Any(word => displayWords.Contains(word.ToLowerInvariant())))
            {
                return "Jobs";
            }
        }
        return "Business";
    }

    /// <summary>
    /// Generates a list of tags based on the provided input, filtering out the company name and adding additional tags
    /// based on the content of the snippet and display link.
    /// </summary>
    /// <remarks>The method filters out the <paramref name="companyName"/> from the provided tags and analyzes
    /// the <paramref name="snippet"/> and <paramref name="displayLink"/> for specific keywords to add contextually
    /// relevant tags. Keywords are categorized into news, program, and job-related terms.</remarks>
    /// <param name="companyName">The name of the company to exclude from the tags.</param>
    /// <param name="tags">A list of initial tags to process. Must not be null or empty.</param>
    /// <param name="snippet">A text snippet used to determine additional tags based on keywords.</param>
    /// <param name="displayLink">A display link used to determine additional tags based on keywords.</param>
    /// <returns>An array of tags after filtering and processing. The array may include additional tags such as "News Update",
    /// "Program", or "Jobs" if relevant keywords are found in the snippet or display link. Returns an empty array if
    /// the input tags are null or empty.</returns>
    public static string[] GetTags(string companyName, List<string> tags, string snippet, string displayLink)
    {
        if (tags == null || tags.Count == 0)
        {
            return [];
        }

        // Case-insensitive comparison for filtering company name
        tags = [.. tags.Where(tag => !string.Equals(tag, companyName, StringComparison.OrdinalIgnoreCase))];
        var result = new List<string>(tags);
        if (string.IsNullOrWhiteSpace(snippet))
        {
            return [.. result];
        }

        var words = snippet.Split(_spltOptions, StringSplitOptions.RemoveEmptyEntries)
                           .Select(w => w.ToLowerInvariant())
                           .ToList();
        var displayWords = displayLink.Split(_spltOptions, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(w => w.ToLowerInvariant())
                                      .ToList();

        if (_newsKeywords.Any(word => words.Contains(word.ToLowerInvariant())) ||
            _newsKeywords.Any(word => displayWords.Contains(word.ToLowerInvariant())))
        {
            result.Add("recent");
        }
        if (_programKeywords.Any(word => words.Contains(word.ToLowerInvariant())) ||
            _programKeywords.Any(word => displayWords.Contains(word.ToLowerInvariant())))
        {
            result.Add("initiative");
        }
        if (_jobKeywords.Any(word => words.Contains(word.ToLowerInvariant())) ||
            _jobKeywords.Any(word => displayWords.Contains(word.ToLowerInvariant())))
        {
            result.Add("careers");
        }
        return [.. result];
    }
}

using Application.Contracts;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

/// <summary>
/// Represents a page model for analyzing company data, providing functionality to retrieve and display company
/// summaries.
/// </summary>
/// <remarks>This class is designed to interact with a company repository to fetch and display a paginated
/// list of company profiles. It initializes with a specified repository and provides an asynchronous method to load
/// the first page of company summaries.</remarks>
public class CompanyAnalysisPage : PageModel
{
    private readonly static List<string> _jobKeywords = ["pay", "career", "careers", "job", "jobs", "position", "employment", "work", "vacancy", "opening", "opportunity", "hiring"];
    private readonly static List<string> _newsKeywords = ["innovation", "win", "partner", "strategic", "news", "update", "announcement", "press release", "report", "article", "story", "coverage", "media"];
    private readonly static List<string> _programKeywords = ["innovations", "compliant", "education", "invest", "design", "government", "collaborate", "support ", "program", "initiative", "project", "campaign", "strategy", "plan", "effort", "action", "activity"];
    private readonly static char[] _spltOptions = [' ', ',', '.', ';', ':', '-', '!', '?', '/', '\\'];

    private readonly IDisplayRepository _displayRepository;
    private readonly ISearch<OrganicResult> _search;
    private readonly ILogger<CompanyAnalysisPage> _logger;

    /// <summary>
    /// The name of the company to display in the header.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the current page number in a paginated list or document.
    /// </summary>
    public int PageNumber { get; private set; } = 1;

    /// <summary>
    /// Gets the number of items to display per page.
    /// </summary>
    public int PageSize { get; private set; } = 1;

    /// <summary>
    /// Gets the total number of pages available.
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Gets the total count of items processed.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// The sections of results displayed on the page, each containing a list of items.
    /// </summary>
    public List<SectionVm> Sections { get; set; } = [];

    /// <summary>
    /// Gets the collection of job summaries.
    /// </summary>
    public List<CompanyProfile> CompanySummaries { get; private set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class with the specified company repository.
    /// </summary>
    /// <param name="repository">The repository used to access company data. Cannot be null.</param>
    /// <param name="search">The search service used to perform searches for company-related data. Cannot be null.</param>
    /// <param name="logger">The logger used for logging information and errors. Cannot be null.</param>
    public CompanyAnalysisPage(
        IDisplayRepository repository,
        ISearch<OrganicResult> search,
        ILogger<CompanyAnalysisPage> logger)
    {
        _displayRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _search = search ?? throw new ArgumentNullException(nameof(search));
    }

    /// <summary>
    /// Asynchronously retrieves a paginated list of company summaries.
    /// </summary>
    /// <remarks>This method fetches the first page of company summaries with a fixed page size of
    /// 25.</remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task OnGetAsync([FromQuery] int page = 1)
    {

        TotalCount = _displayRepository.GetJobCount(DateTime.Now.AddDays(-30));
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
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

        CompanyName = result.CompanyName;
        Sections.Clear();
        Sections = [.. GetCompanyResults(result, _search)
            .GroupBy(item => item.Type)
            .Select(group => new SectionVm
            {
                Title = group.Key,
                Items = [.. group]
            })];

        _logger.LogInformation("Retrieved {Count} company summaries for page {PageNumber}.", CompanySummaries.Count, PageNumber);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a list of search results related to a company's data analytics and services.
    /// </summary>
    /// <remarks>The method fetches up to 10 organic search results based on a query that focuses on the
    /// company's  data analytics and services. Each result includes details such as the title, URL, source, and
    /// snippet.</remarks>
    /// <param name="companyProfile">The profile of the company, including its name, to be used in the search query.</param>
    /// <param name="search">An implementation of <see cref="ISearch{OrganicResult}"/> used to perform the search operation.</param>
    /// <returns>A list of <see cref="OrganicResultItem"/> objects representing the top search results.  Returns an empty list if no
    /// results are found.</returns>
    public static List<OrganicResultItem> GetCompanyResults(CompanyProfile companyProfile, ISearch<OrganicResult> search)
    {
        List<OrganicResultItem> resultItemVms = new List<OrganicResultItem>();
        var result = search.FetchOrganicResults($"All recent information on the company \"{companyProfile.CompanyName}\", focusing on Data analytics and services.", "NC");
        if (result == null || result.Result == null || !result.Result.Any())
        {
            return resultItemVms;
        }

        foreach (var item in result.Result.Take(10))
        {
            resultItemVms.Add(new OrganicResultItem
            {
                Title = item.Title,
                Url = item.Link,
                Source = item.Source,
                DisplayedLink = item.DisplayedLink,
                Date = item.Date,
                Snippet = item.Snippet,
                Type = GetTitle(item.Snippet, item.Link.ToString()),
                Tags = GetTags(companyProfile.CompanyName, item.SnippetHighlightedWords, item.Snippet, item.Link.ToString())
            });
        }
        return resultItemVms;
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

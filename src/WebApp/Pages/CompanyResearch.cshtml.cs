using Application.Contracts;
using Application.Models;

namespace WebApp.Pages;

/// <summary>
/// Represents the model for a company research page, providing data such as company summaries, profiles, and pagination
/// details.
/// </summary>
/// <remarks>This model is used in an ASP.NET Core Razor Pages application to display company research data. It
/// includes functionality for retrieving and managing paginated company summaries and profiles.</remarks>
public sealed class CompanyResearchModel : PageModel
{
    private readonly IDisplayRepository _displayRepository;
    private readonly ILogger<CompanyResearchModel> _logger;

    /// <summary>
    /// Gets the current page number in a paginated list or document.
    /// </summary>
    public int PageNumber { get; private set; } = 1;

    /// <summary>
    /// Gets the collection of job summaries.
    /// </summary>
    public List<CompanyProfile> CompanySummaries { get; set; } = [];

    /// <summary>
    /// Gets the total count of items processed.
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// The sections of results displayed on the page, each containing a list of items.
    /// </summary>
    public List<CompanyProfileResult> Profiles { get; set; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyResearchModel"/> class,  which provides data for company
    /// research, including summaries and profiles.
    /// </summary>
    /// <param name="repository">The repository used to retrieve company data, such as job counts and paginated company summaries. Cannot be
    /// <see langword="null"/>.</param>
    /// <param name="logger">The logger used to log diagnostic and operational information. Cannot be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="repository"/> or <paramref name="logger"/> is <see langword="null"/>.</exception>
    public CompanyResearchModel(
        IDisplayRepository repository,
        ILogger<CompanyResearchModel> logger)
    {
        _displayRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var sinceDate = DateTime.UtcNow.AddDays(-30);
        TotalCount = _displayRepository.GetJobCount(sinceDate);

        var companies = _displayRepository.GetPaginatedCompanies(sinceDate, PageNumber, TotalCount);
        if (companies is not null && companies.Any())
        {
            CompanySummaries = [.. companies];
            Profiles = [.. companies.Select((item, index) => new CompanyProfileResult(item, index))];
        }
    }

    /// <summary>
    /// Handles GET requests asynchronously and performs any necessary initialization or logging.
    /// </summary>
    /// <remarks>Logs a warning if no company summaries are available. This method is typically
    /// invoked  as part of a page lifecycle in an ASP.NET Core Razor Pages application.</remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task OnGetAsync()
    {
        if (CompanySummaries == null || CompanySummaries.Count == 0)
        {
            _logger.LogWarning("No company summaries found.");
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents the result of a company profile query, including the profile data and its position in a sequence.
/// </summary>
/// <remarks>The <see cref="CompanyProfileResult"/> class encapsulates a company profile and its
/// associated index in a sequence. The index is adjusted to be 1-based for easier readability in user-facing
/// contexts.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="CompanyProfileResult"/> class with the specified company
/// profile and index.
/// </remarks>
/// <remarks>The <paramref name="index"/> parameter is adjusted by adding 1 to represent a
/// one-based index in the <see cref="CurrentIndex"/> property.</remarks>
/// <param name="companyProfile">The company profile associated with this result. Cannot be <see langword="null"/>.</param>
/// <param name="index">The zero-based index of the company profile. The resulting <see cref="CurrentIndex"/> will be this value
/// incremented by 1.</param>
public sealed class CompanyProfileResult(CompanyProfile companyProfile, int index)
{
    /// <summary>
    /// Gets or sets the company profile information.
    /// </summary>
    public CompanyProfile CompanyProfile { get; set; } = companyProfile;

    /// <summary>
    /// Gets the current index within the collection.
    /// </summary>
    public int CurrentIndex { get; } = index + 1;
}

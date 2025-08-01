using Application.Contracts;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    /// <summary>
    /// Represents a page model for analyzing company data, providing functionality to retrieve and display company
    /// summaries.
    /// </summary>
    /// <remarks>This class is designed to interact with a company repository to fetch and display a paginated
    /// list of company profiles. It initializes with a specified repository and provides an asynchronous method to load
    /// the first page of company summaries.</remarks>
    public class CompanyAnalysisPage : PageModel
    {
        private readonly IDisplayRepository _displayRepository;
        private readonly ILogger<CompanyAnalysisPage> _logger;

        /// <summary>
        /// Gets the current page number in a paginated list or document.
        /// </summary>
        public int PageNumber { get; private set; } = 1;

        /// <summary>
        /// Gets the number of items to display per page.
        /// </summary>
        public int PageSize { get; private set; } = 3;

        /// <summary>
        /// Gets the total number of pages available.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Gets the total count of items processed.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Gets the collection of job summaries.
        /// </summary>
        public List<CompanyProfile> CompanySummaries { get; private set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class with the specified company repository.
        /// </summary>
        /// <param name="repository">The repository used to access company data. Cannot be null.</param>
        /// <param name="logger">The logger used for logging information and errors. Cannot be null.</param>
        public CompanyAnalysisPage(IDisplayRepository repository, ILogger<CompanyAnalysisPage> logger)
        {
            _displayRepository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of company summaries.
        /// </summary>
        /// <remarks>This method fetches the first page of company summaries with a fixed page size of
        /// 25.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task OnGetAsync([FromQuery] int page = 1)
        {
            PageNumber = page < 1 ? 1 : page;
            TotalCount = _displayRepository.GetJobCount(DateTime.Now.AddDays(-30));
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            CompanySummaries = [.. _displayRepository.GetPaginatedCompanies(DateTime.Now.AddDays(-30), PageNumber, PageSize)];

            if (CompanySummaries == null || CompanySummaries.Count == 0)
            {
                _logger.LogWarning("No company summaries found.");
                return;
            }
            _logger.LogInformation("Retrieved {Count} company summaries for page {PageNumber}.", CompanySummaries.Count, PageNumber);
            await Task.CompletedTask;
        }
    }
}

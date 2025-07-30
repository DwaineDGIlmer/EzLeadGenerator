using Application.Contracts;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    /// <summary>
    /// Represents the model for the company job analysis page.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IDisplayRepository _displayRepository;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        /// Gets the current page number in a paginated list or document.
        /// </summary>
        public int PageNumber { get; private set; } = 1;

        /// <summary>
        /// Gets the number of items to display per page.
        /// </summary>
        public int PageSize { get; private set; } = 4;

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
        public List<JobSummary> JobSummaries { get; private set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class with the specified display repository and
        /// logger.
        /// </summary>
        /// <param name="displayRepository">The repository used to retrieve display data. Cannot be <see langword="null"/>.</param>
        /// <param name="logger">The logger used for logging operations. Cannot be <see langword="null"/>.</param>
        public IndexModel(
            IDisplayRepository displayRepository,
            ILogger<IndexModel> logger)
        {
            ArgumentNullException.ThrowIfNull(displayRepository, nameof(displayRepository));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _logger = logger;
            _displayRepository = displayRepository;
        }

        /// <summary>
        /// Asynchronously retrieves and sets a paginated list of job summaries from the repository.
        /// </summary>
        /// <remarks>This method fetches job summaries from the past 30 days, starting from the current
        /// date. The results are limited to the first page with a maximum of 25 entries.</remarks>
        /// <returns></returns>
        public async Task OnGetAsync([FromQuery] int page = 1)
        {
            PageNumber = page < 1 ? 1 : page;
            TotalCount = _displayRepository.GetJobCount(DateTime.UtcNow.AddDays(-30));
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            JobSummaries = [.. _displayRepository.GetPaginatedJobs(DateTime.Now.AddDays(-30), PageNumber, PageSize)];

            if (JobSummaries == null || JobSummaries.Count == 0)
            {
                _logger.LogWarning("No job summaries found.");
                return;
            }
            _logger.LogInformation("Retrieved {Count} job summaries for page {PageNumber}.", JobSummaries.Count, PageNumber);
            await Task.CompletedTask;
        }
    }
}

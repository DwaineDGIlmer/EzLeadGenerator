using Application.Contracts;
using Application.Models;
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
        /// UTC date. The results are limited to the first page with a maximum of 25 entries.</remarks>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            var jobSummaries = _displayRepository.GetPaginatedJobs(DateTime.UtcNow.AddDays(-30), 0, 25);
            JobSummaries.AddRange(jobSummaries);
            if (JobSummaries == null || JobSummaries.Count == 0)
            {
                _logger.LogWarning("No job summaries found.");
            }
            await Task.Delay(1000);
        }
    }
}

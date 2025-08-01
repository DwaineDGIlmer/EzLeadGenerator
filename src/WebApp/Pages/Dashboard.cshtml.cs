using Application.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    /// <summary>
    /// Represents the data model for the dashboard page, providing aggregated statistics and collections of data
    /// related to agencies, companies, jobs, and other metrics.
    /// </summary>
    /// <remarks>This model is used to populate the dashboard view with key metrics and data visualizations.
    /// It includes properties for total counts, collections of detailed data, and calculated metrics.</remarks>
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly IDisplayRepository _displayRepository;

        /// <summary>
        /// Gets the total number of agencies.
        /// </summary>
        public int TotalAgencies { get; private set; }

        /// <summary>
        /// Gets the total number of jobs processed.
        /// </summary>
        public int TotalJobs { get; private set; }

        /// <summary>
        /// Gets the total number of companies currently tracked.
        /// </summary>
        public int TotalCompanies { get; private set; }

        /// <summary>
        /// Gets or sets the collection of company agency rows.
        /// </summary>
        public List<CompanyAgencyRow> CompanyAgencyTable { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of top companies.
        /// </summary>
        public List<TopCompany> TopCompanies { get; set; } = [];

        /// <summary>
        /// Gets or sets the distribution of job titles within a collection.
        /// </summary>
        public List<JobTitleDistribution> JobTitleDistribution { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardModel"/> class.
        /// </summary>
        /// <param name="logger">The logger instance used for logging diagnostic and operational messages.</param>
        /// <param name="displayRepository">The repository used to retrieve display-related data.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> or <paramref name="displayRepository"/> is <see langword="null"/>.</exception>
        public DashboardModel(ILogger<DashboardModel> logger, IDisplayRepository displayRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _displayRepository = displayRepository ?? throw new ArgumentNullException(nameof(displayRepository));
        }

        /// <summary>
        /// Handles the GET request to load and prepare dashboard data for display.
        /// </summary>
        /// <remarks>This method retrieves job data from the repository for the past 30 days, calculates
        /// various statistics such as the total number of companies, agencies, and jobs, and prepares grouped and
        /// aggregated data for display on the dashboard. The method also logs a message upon successful data
        /// loading.</remarks>
        public void OnGet()
        {
            var jobs = _displayRepository.GetPaginatedJobsAsync(DateTime.UtcNow.AddDays(-30), 0, 100).Result.ToList();
            TotalCompanies = _displayRepository.GetCompanyCount(DateTime.UtcNow.AddDays(-30));
            TotalAgencies = jobs.Select(j => j.HiringAgency).Distinct().Count();
            TotalJobs = jobs.Count;
            CompanyAgencyTable = [.. jobs
                .GroupBy(j => new { j.CompanyName, j.HiringAgency })
                .Select(g => new CompanyAgencyRow
                {
                    CompanyName = g.Key.CompanyName,
                    HiringAgency = g.Key.HiringAgency,
                    JobCount = g.Count(),
                    LatestPosting = g.Max(j => j.CreatedAt)
                })
                .OrderByDescending(c => c.JobCount)];
            TopCompanies = [.. jobs
                .GroupBy(j => j.CompanyName)
                .Select(g => new TopCompany
                {
                    CompanyName = g.Key,
                    JobCount = g.Count()
                })
                .OrderByDescending(c => c.JobCount)
                .Take(10)];
            JobTitleDistribution = [.. jobs
            .GroupBy(j => j.JobTitle)
            .Select(g => new JobTitleDistribution
            {
                JobTitle = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(jtd => jtd.Count)];

            _logger.LogInformation("Dashboard data loaded successfully.");
        }
    }

    /// <summary>
    /// Represents a row of data containing information about a company and its associated hiring agency.
    /// </summary>
    /// <remarks>This class is typically used to store and transfer information about a company's hiring
    /// agency,  the number of job postings, and the date of the latest job posting.</remarks>
    public class CompanyAgencyRow
    {
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the agency responsible for hiring.
        /// </summary>
        public string HiringAgency { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of jobs currently being tracked.
        /// </summary>
        public int JobCount { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the most recent posting.
        /// </summary>
        public DateTime LatestPosting { get; set; }
    }

    /// <summary>
    /// Represents a company with its name and the number of job openings.
    /// </summary>
    public class TopCompany
    {
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the total number of jobs currently being tracked.
        /// </summary>
        public int JobCount { get; set; }
    }

    /// <summary>
    /// Represents the distribution of a specific job title, including the job title itself and the count of
    /// occurrences.
    /// </summary>
    /// <remarks>This class is typically used to store and analyze data related to job title frequencies, such
    /// as in reporting or visualization scenarios.</remarks>
    public class JobTitleDistribution
    {
        /// <summary>
        /// Gets or sets the job title of the employee.
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the count of items.
        /// </summary>
        public int Count { get; set; }
    }
}

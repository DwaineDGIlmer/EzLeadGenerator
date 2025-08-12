using Application.Contracts;
using Application.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    /// <summary>
    /// Provides methods to asynchronously retrieve and manage job summaries and company profiles.
    /// </summary>
    /// <remarks>The <see cref="DisplayRepository"/> class is responsible for loading and providing access to
    /// job summaries and company profiles. It initializes by asynchronously loading all jobs and companies from the
    /// specified repositories. The class supports paginated retrieval of job summaries and company profiles, allowing
    /// clients to efficiently access large datasets.</remarks>
    public class DisplayRepository : IDisplayRepository
    {
        private readonly ILogger<DisplayRepository> _logger;
        private readonly List<JobSummary> _allJobs = [];
        private readonly List<CompanyProfile> _allCompanies = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayRepository"/> class.
        /// </summary>
        /// <remarks>This constructor initializes the repository and begins asynchronous loading of all
        /// jobs and companies.</remarks>
        /// <param name="companyRepository">The repository used to access company data. Cannot be <see langword="null"/>.</param>
        /// <param name="jobsRepository">The repository used to access job data. Cannot be <see langword="null"/>.</param>
        /// <param name="logger">The logger used for logging operations within the repository. Cannot be <see langword="null"/>.</param>
        public DisplayRepository(
            ICompanyRepository companyRepository,
            IJobsRepository jobsRepository,
            ILogger<DisplayRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(companyRepository, nameof(companyRepository));
            ArgumentNullException.ThrowIfNull(jobsRepository, nameof(jobsRepository));

            _logger = logger;
            LoadAllJobs(jobsRepository, _allJobs, _logger);
            LoadAllCompanies(companyRepository, _allJobs, _allCompanies, _logger);
        }

        /// <summary>
        /// Gets the count of jobs posted on or after the specified date.
        /// </summary>
        /// <param name="fromDate">The date from which to start counting jobs. Only jobs posted on or after this date are included in the
        /// count.</param>
        /// <returns>The number of jobs posted on or after the specified date.</returns>
        public int GetJobCount(DateTime fromDate)
        {
            return _allJobs
                    .Count(j => j.CreatedAt >= fromDate);
        }

        /// <summary>
        /// Gets the count of companies created on or after the specified date.
        /// </summary>
        /// <param name="fromDate">The date from which to start counting companies. Only companies created on or after this date are included
        /// in the count.</param>
        /// <returns>The number of companies created on or after the specified date.</returns>
        public int GetCompanyCount(DateTime fromDate)
        {
            return _allCompanies
                    .Count(c => c.CreatedAt >= fromDate);
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of job summaries posted on or after the specified date.
        /// </summary>
        /// <remarks>This method reads job summary files from a predefined directory, deserializes them,
        /// and filters the jobs based on the specified date. It then orders the jobs by their posted date in descending
        /// order and returns the specified page of results. If an error occurs while reading a file, the error is
        /// logged, and the method continues processing the remaining files.</remarks>
        /// <param name="fromDate">The date from which to start retrieving job summaries. Only jobs posted on or after this date will be
        /// included.</param>
        /// <param name="page">The zero-based page index to retrieve. Must be a non-negative integer.</param>
        /// <param name="pageSize">The number of job summaries to include in each page. Must be a positive integer.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="JobSummary"/>
        /// objects for the specified page.</returns>
        public Task<IEnumerable<JobSummary>> GetPaginatedJobsAsync(DateTime fromDate, int page, int pageSize)
        {
            var allJobs = GetPaginatedJobs(fromDate, page, pageSize);
            return Task.FromResult(allJobs);
        }

        /// <summary>
        /// Retrieves a paginated list of job summaries posted on or after the specified date.
        /// </summary>
        /// <remarks>The jobs are ordered by their posted date in descending order. Ensure that the
        /// <paramref name="page"/> and <paramref name="pageSize"/> parameters are set to retrieve the desired subset of
        /// results.</remarks>
        /// <param name="fromDate">The date from which to start retrieving job summaries. Only jobs posted on or after this date are included.</param>
        /// <param name="page">The page number of results to retrieve. Must be greater than zero.</param>
        /// <param name="pageSize">The number of job summaries to include in each page. Must be greater than zero.</param>
        /// <returns>An enumerable collection of <see cref="JobSummary"/> objects representing the jobs for the specified page.</returns>
        public IEnumerable<JobSummary> GetPaginatedJobs(DateTime fromDate, int page, int pageSize)
        {
            return [.. _allJobs
                    .OrderByDescending(j => j.PostedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)];
        }

        /// <summary>
        /// Asynchronously retrieves a paginated list of company profiles that have been updated since the specified
        /// date.
        /// </summary>
        /// <remarks>This method reads JSON files from a specified directory, deserializes them into
        /// company profiles, and filters them based on the provided date. It logs any errors encountered during file
        /// reading or JSON deserialization.</remarks>
        /// <param name="fromDate">The date from which to filter company profiles based on their last job synchronization date.</param>
        /// <param name="page">The page number to retrieve. Must be greater than or equal to 1.</param>
        /// <param name="pageSize">The number of company profiles to include in each page. Must be greater than 0.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of <see
        /// cref="CompanyProfile"/> objects, ordered by their last job synchronization date in descending order.</returns>
        public Task<IEnumerable<CompanyProfile>> GetPaginatedCompaniesAsync(DateTime fromDate, int page, int pageSize)
        {
            var allProfiles = GetPaginatedCompanies(fromDate, page, pageSize);
            return Task.FromResult(allProfiles);
        }

        /// <summary>
        /// Retrieves a paginated list of company profiles updated on or after the specified date.
        /// </summary>
        /// <remarks>The company profiles are ordered by their update date in descending order, ensuring
        /// the most recently updated profiles appear first.</remarks>
        /// <param name="fromDate">The date from which to start retrieving company profiles. Only profiles updated on or after this date are
        /// included.</param>
        /// <param name="page">The page number to retrieve. Must be greater than zero.</param>
        /// <param name="pageSize">The number of company profiles to include in each page. Must be greater than zero.</param>
        /// <returns>An enumerable collection of <see cref="CompanyProfile"/> objects representing the requested page of company
        /// profiles.</returns>
        public IEnumerable<CompanyProfile> GetPaginatedCompanies(DateTime fromDate, int page, int pageSize)
        {
            return [.. _allCompanies
                    .OrderBy(c => c.CompanyName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)];
        }

        /// <summary>
        /// Asynchronously loads all job profiles from the specified repository for the past 30 days.
        /// </summary>
        /// <remarks>This method retrieves job profiles from the past 30 days and adds them to the
        /// internal collection. It logs the number of profiles loaded or any errors encountered during the
        /// operation.</remarks>
        /// <param name="jobsRepository">The repository from which to retrieve job profiles. Cannot be null.</param>
        /// <param name="allJobs">List of job summaries to populate with the loaded data.</param>
        /// <param name="logger">Logger used for logging operations within the repository. Cannot be null.</param>
        /// <returns></returns>
        public static void LoadAllJobs(IJobsRepository jobsRepository, List<JobSummary> allJobs, ILogger logger)
        {
            try
            {
                var jobs = jobsRepository.GetJobsAsync(DateTime.Now.AddDays(-30)).Result;
                allJobs.AddRange(jobs);
                logger.LogInformation("Successfully loaded {CompanyCount} company profiles", jobs.Count());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading company profiles");
            }
        }

        /// <summary>
        /// Asynchronously loads all company profiles from the specified repository and adds them to the internal
        /// collection.
        /// </summary>
        /// <remarks>This method retrieves company profiles from the past 30 days and logs the number of
        /// profiles loaded. If an error occurs during the loading process, it logs the error.</remarks>
        /// <param name="companyRepository">The repository from which to retrieve company profiles. Cannot be null.</param>
        /// <param name="allCompanies">List of company profiles to populate with the loaded data.</param>
        /// <param name="allJobs">List of job summaries to use for retrieving company profiles.</param>
        /// <param name="logger">The logger used for logging operations within the repository. Cannot be null.</param>
        /// <returns></returns>
        public static void LoadAllCompanies(ICompanyRepository companyRepository, List<JobSummary> allJobs, List<CompanyProfile> allCompanies, ILogger logger)
        {
            try
            {
                foreach (var job in allJobs)
                {
                    var companyProfile = companyRepository.GetCompanyProfileAsync(job.CompanyId).Result;
                    if (companyProfile != null &&
                        !allCompanies.Exists(job => job.CompanyId == companyProfile.CompanyId))
                    {
                        allCompanies.Add(companyProfile);
                    }
                }
                logger.LogInformation("Successfully loaded {CompanyCount} company profiles", allCompanies.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading company profiles");
            }
        }
    }
}

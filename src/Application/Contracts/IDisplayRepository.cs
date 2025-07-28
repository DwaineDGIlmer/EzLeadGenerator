using Application.Models;

namespace Application.Contracts
{
    /// <summary>
    /// Provides methods for accessing and managing company-related data.
    /// </summary>
    /// <remarks>This interface defines operations for retrieving and saving company profiles and job
    /// summaries. Implementations should ensure thread safety and handle data access exceptions
    /// appropriately.</remarks>
    public interface IDisplayRepository
    {
        /// <summary>
        /// Retrieves a paginated list of companies associated with recent Data Engineer job postings.
        /// </summary>
        /// <param name="fromDate">The date from which to start retrieving job summaries. Only jobs created on or after this date will be
        /// included.</param>
        /// <param name="page">The zero-based page index to retrieve. Must be non-negative.</param>
        /// <param name="pageSize">The number of job summaries to include in each page. Must be greater than zero.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="JobSummary"/>
        /// objects for the specified page.</returns>
        Task<IEnumerable<CompanyProfile>> GetPaginatedCompaniesAsync(DateTime fromDate, int page, int pageSize);

        /// <summary>
        /// Retrieves a paginated list of company profiles that have been updated since the specified date.
        /// </summary>
        /// <param name="fromDate">The date from which to start retrieving updated company profiles. Only profiles updated after this date are
        /// included.</param>
        /// <param name="page">The page number to retrieve. Must be a non-negative integer.</param>
        /// <param name="pageSize">The number of company profiles to include in each page. Must be a positive integer.</param>
        /// <returns>An enumerable collection of <see cref="CompanyProfile"/> objects representing the company profiles for the
        /// specified page.</returns>
        IEnumerable<CompanyProfile> GetPaginatedCompanies(DateTime fromDate, int page, int pageSize);

        /// <summary>
        /// Asynchronously retrieves a paginated list of job summaries starting from a specified date.
        /// </summary>
        /// <param name="fromDate">The date from which to start retrieving job summaries. Only jobs created on or after this date will be
        /// included.</param>
        /// <param name="page">The page number to retrieve. Must be a non-negative integer.</param>
        /// <param name="pageSize">The number of job summaries to include in each page. Must be a positive integer.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="JobSummary"/>
        /// objects for the specified page.</returns>
        Task<IEnumerable<JobSummary>> GetPaginatedJobsAsync(DateTime fromDate, int page, int pageSize);

        /// <summary>
        /// Retrieves a paginated list of job summaries starting from a specified date.
        /// </summary>
        /// <param name="fromDate">The date from which to start retrieving job summaries. Only jobs created on or after this date will be
        /// included.</param>
        /// <param name="page">The zero-based page index to retrieve. Must be non-negative.</param>
        /// <param name="pageSize">The number of job summaries to include in each page. Must be greater than zero.</param>
        /// <returns>An enumerable collection of <see cref="JobSummary"/> objects representing the jobs in the specified page.
        /// The collection will be empty if no jobs match the criteria.</returns>
        IEnumerable<JobSummary> GetPaginatedJobs(DateTime fromDate, int page, int pageSize);
    }
}

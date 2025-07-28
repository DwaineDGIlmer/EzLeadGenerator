using Application.Models;

namespace Application.Contracts
{
    /// <summary>
    /// Provides methods for accessing and managing company-related data.
    /// </summary>
    /// <remarks>This interface defines operations for retrieving and saving company profiles and job
    /// summaries. Implementations should ensure thread safety and handle data access exceptions
    /// appropriately.</remarks>
    public interface ICompanyRepository
    {
        /// <summary>
        /// Asynchronously retrieves the profile of a company based on the specified company name.
        /// </summary>
        /// <param name="companyName">The unique name of the company whose profile is to be retrieved. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="CompanyProfile"/>
        /// of the specified company.</returns>
        Task<CompanyProfile?> GetCompanyProfileAsync(string companyName);

        /// <summary>
        /// Asynchronously retrieves a list of company profiles that have been updated since the specified date.
        /// </summary>
        /// <param name="fromDate">The date from which to retrieve updated company profiles. Only profiles updated on or after this date will
        /// be included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
        /// cref="CompanyProfile"/> objects.</returns>
        Task<IEnumerable<CompanyProfile>> GetCompanyProfileAsync(DateTime fromDate);

        /// <summary>
        /// Asynchronously saves the specified company profile to the data store.
        /// </summary>
        /// <param name="profile">The <see cref="CompanyProfile"/> object containing the profile data to be saved. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task AddCompanyProfileAsync(CompanyProfile profile);

        /// <summary>
        /// Updates the company information asynchronously based on the provided profile summary.
        /// </summary>
        /// <param name="profile">The <see cref="JobSummary"/> object containing the updated job details.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task UpdateCompanyProfileAsync(CompanyProfile profile);

        /// <summary>
        /// Asynchronously deletes the specified company profile
        /// </summary>
        /// <param name="profile">The <see cref="JobSummary"/> representing the job to be deleted. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteCompanyProfileAsync(CompanyProfile profile);
    }
}

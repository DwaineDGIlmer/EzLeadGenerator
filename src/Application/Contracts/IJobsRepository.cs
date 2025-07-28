using Application.Models;

namespace Application.Contracts;

/// <summary>
/// Provides methods for accessing and managing company-related data.
/// </summary>
/// <remarks>This interface defines operations for retrieving and saving company profiles and job
/// summaries. Implementations should ensure thread safety and handle data access exceptions
/// appropriately.</remarks>
public interface IJobsRepository
{
    /// <summary>
    /// Asynchronously retrieves a list of job summaries for the specified company.
    /// </summary>
    /// <param name="JobId">The unique identifier of the company for which to retrieve job summaries. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
    /// cref="JobSummary"/> objects representing the jobs associated with the specified company. The list will be
    /// empty if no jobs are found.</returns>
    Task<JobSummary?> GetJobsAsync(string JobId);

    /// <summary>
    /// Asynchronously retrieves a list of job summaries that have been created or updated since the specified date.
    /// </summary>
    /// <param name="fromDate">The date from which to retrieve job summaries. Only jobs created or updated on or after this date will be
    /// included.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see
    /// cref="JobSummary"/> objects.</returns>
    Task<IEnumerable<JobSummary>> GetJobsAsync(DateTime fromDate);

    /// <summary>
    /// Asynchronously saves the specified job profile.
    /// </summary>
    /// <param name="profile">The job profile to be saved. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task AddJobAsync(JobSummary profile);

    /// <summary>
    /// Updates the job information asynchronously based on the provided job summary.
    /// </summary>
    /// <param name="profile">The <see cref="JobSummary"/> object containing the updated job details.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateJobAsync(JobSummary profile);

    /// <summary>
    /// Asynchronously deletes the specified job.
    /// </summary>
    /// <param name="profile">The <see cref="JobSummary"/> representing the job to be deleted. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteJobAsync(JobSummary profile);
}

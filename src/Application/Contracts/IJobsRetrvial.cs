namespace Application.Contracts
{
    /// <summary>
    /// Defines a contract for retrieving job listings based on search criteria.
    /// </summary>
    /// <remarks>Implementations of this interface should provide mechanisms to query job listings using
    /// specified search terms and geographical locations.</remarks>
    public interface IJobsRetrieval<T>
    {
        /// <summary>
        /// Asynchronously retrieves job listings based on the specified query and location.
        /// </summary>
        /// <param name="query">The search term used to filter job listings. Cannot be null or empty.</param>
        /// <param name="location">The geographical location to search for jobs. Cannot be null or empty.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a JSON string of job listings that
        /// match the specified criteria.</returns>
        Task<IEnumerable<T>> FetchJobs(string query, string location);
    }
}

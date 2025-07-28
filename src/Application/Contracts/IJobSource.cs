namespace Application.Contracts
{
    /// <summary>
    /// Represents a source of jobs, providing methods to manage and retrieve job source information asynchronously.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for defining the specific behavior of job
    /// source management, including how job sources are retrieved, added, and updated. The interface provides
    /// asynchronous methods to facilitate non-blocking operations, which are essential for scalable
    /// applications.</remarks>
    public interface IJobSourceService
    {
        /// <summary>
        /// Gets or sets the unique identifier for the job source.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets or sets the description of the job source.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Updates the company profiles asynchronously.
        /// </summary>
        /// <remarks>This method performs an asynchronous update of company profiles. It returns a boolean
        /// value indicating the success of the operation.</remarks>
        /// <returns><see langword="true"/> if the update operation was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> UpdateCompanyProfilesAsync();

        /// <summary>
        /// Updates the job source with the specified data asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the
        /// update was successful; otherwise, <see langword="false"/>.</returns>
        public Task<bool> UpdateJobSourceAsync();
    }
}

using Application.Models;

namespace Application.Contracts
{
    /// <summary>
    /// Defines a contract for fetching organic search results based on a query and location.
    /// </summary>
    public interface ISearch
    {
        /// <summary>
        /// Retrieves a collection of organic search results based on the specified query and location.
        /// </summary>
        /// <param name="query">The search query string used to find relevant organic results.</param>
        /// <param name="location">The geographical location to tailor the search results to.</param>
        /// <returns>An enumerable collection of <see cref="OrganicResult"/> objects representing the search results.</returns>
        public Task<IEnumerable<T>?> FetchOrganicResults<T>(string query, string location);

        /// <summary>
        /// Asynchronously fetches search results based on the specified query and location.
        /// </summary>
        /// <remarks>The method performs an asynchronous operation to retrieve search results. The caller
        /// should ensure that the query and location parameters are valid and non-empty to avoid exceptions. The
        /// returned collection may be empty if no matches are found.</remarks>
        /// <param name="query">The search query string. Cannot be null or empty.</param>
        /// <param name="location">The location to scope the search results. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
        /// items of type <typeparamref name="T"/> matching the search criteria, or <see langword="null"/> if no results
        /// are found.</returns>
        public Task<IEnumerable<T>?> FetchSearchResults<T>(string query, string location);
    }
}

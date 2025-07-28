using Application.Models;

namespace Application.Contracts
{
    /// <summary>
    /// Defines a contract for fetching organic search results based on a query and location.
    /// </summary>
    public interface ISearch<T>
    {
        /// <summary>
        /// Retrieves a collection of organic search results based on the specified query and location.
        /// </summary>
        /// <param name="query">The search query string used to find relevant organic results.</param>
        /// <param name="location">The geographical location to tailor the search results to.</param>
        /// <returns>An enumerable collection of <see cref="OrganicResult"/> objects representing the search results.</returns>
        public Task<IEnumerable<T>?> FetchOrganicResults(string query, string location);
    }
}

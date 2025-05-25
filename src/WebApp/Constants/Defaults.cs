namespace WebApp.Constants
{
    /// <summary>
    /// Provides default values and utility methods for configuration and settings management.
    /// </summary>
    /// <remarks>This class contains constants representing default environment variable names, file names,
    /// MIME types,  and settings keys used throughout the application. It also provides a method to retrieve formatted 
    /// settings keys for known configuration values.</remarks>
    public static class Defaults
    {
        /// <summary>
        /// Represents the environment variable key for the Search API URL.
        /// </summary>
        /// <remarks>This constant is used to retrieve the URL of the Search API from the environment
        /// variables.</remarks>
        public const string EnvSearchApiUrl = "SEARCH_SERPAPI_API_URL";

        /// <summary>
        /// Represents the environment variable name for the Search API key.
        /// </summary>
        /// <remarks>This constant is used to retrieve the API key for the Search service from the
        /// environment variables. Ensure that the environment variable <see cref="EnvSearchApiKey"/> is set with a
        /// valid API key before using the Search service.</remarks>
        public const string EnvSearchApiKey = "SEARCH_SERPAPI_API_KEY";

        /// <summary>
        /// The default file name used for storing lead data.
        /// </summary>
        public const string LeadFileName = "leads.csv";

        /// <summary>
        /// Represents the MIME type for CSV (Comma-Separated Values) files.
        /// </summary>
        /// <remarks>This constant can be used to specify or identify the MIME type for CSV files in HTTP
        /// headers, file uploads, or other contexts where MIME types are required.</remarks>
        public const string CsvMimeType = "text/csv";

        /// <summary>
        /// Represents the MIME type for JSON content.
        /// </summary>
        /// <remarks>This constant can be used to specify or compare the MIME type for JSON data in HTTP
        /// requests or responses.</remarks>
        public const string JsonMimeType = "application/json";

        /// <summary>
        /// Represents the configuration key for EzLeads settings.
        /// </summary>
        /// <remarks>This constant can be used to reference the EzLeads settings key in configuration
        /// files or settings management systems.</remarks>
        public const string EzLeadsSettings = nameof(EzLeadsSettings);

        /// <summary>
        /// Represents the name of the API key used for search operations.
        /// </summary>
        /// <remarks>This constant can be used as a key or identifier for accessing the search API key in
        /// configuration or settings. The value of this constant is the string "SearchApiKey".</remarks>
        public const string SearchApiKey = nameof(SearchApiKey);

        /// <summary>
        /// Represents the name of the search endpoint as a constant string.
        /// </summary>
        /// <remarks>This constant can be used to reference the search endpoint in a consistent and
        /// type-safe manner throughout the application.</remarks>
        public const string SearchEndpoint = nameof(SearchEndpoint);

        /// <summary>
        /// Retrieves the value of a specified settings key if it is a recognized constant.
        /// </summary>
        /// <param name="settings">The settings key to retrieve. Must be one of the predefined constants:  <c>EzLeadsSettings</c>,
        /// <c>SearchApiKey</c>, or <c>SearchEndpoint</c>.</param>
        /// <returns>A string in the format "<c>EzLeadsSettings:settings</c>", where <c>settings</c> is the provided key.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="settings"/> is not a recognized constant.</exception>
        public static string GetSettings(string settings)
        {
            // Only allow known constants
            return settings switch
            {
                EzLeadsSettings or SearchApiKey or SearchEndpoint => $"{EzLeadsSettings}:{settings}",
                _ => throw new ArgumentException($"Invalid settings key: {settings}", nameof(settings)),
            };
        }
    }
}

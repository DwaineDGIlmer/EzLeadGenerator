using Application.Constants;

namespace Application.Configurations
{
    /// <summary>
    /// Represents the configuration settings for Azure services, including storage operations.
    /// </summary>
    /// <remarks>This class is used to configure various settings required for interacting with Azure
    /// services. It includes properties such as the name of the Azure Table used for storage operations.</remarks>
    public class AzureSettings
    {
        /// <summary>
        /// Gets or sets the name of the Azure Table used for storage operations.
        /// </summary>
        public string AzureTableName { get; set; } = Defaults.AzureTableName;

        /// <summary>
        /// Gets or sets the cache expiration time, in minutes.
        /// </summary>
        public int CacheExpirationInMinutes { get; set; } = Defaults.CacheExpirationInMinutes;

        /// <summary>
        /// Gets or sets the name of the table that stores company profile information.
        /// </summary>
        public string CompanyProfilePartionKey { get; set; } = Defaults.CompanyProfilePartionKey;

        /// <summary>
        /// Gets or sets the name of the Azure table used for storing job summaries.
        /// </summary>
        public string JobSummaryPartionKey { get; set; } = Defaults.JobSummaryPartionKey;

        /// <summary>
        /// Gets or sets the name of the table that stores company profile information.
        /// </summary>
        public string CompanyProfileTableName { get; set; } = Defaults.CompanyProfileTableName;

        /// <summary>
        /// Gets or sets the name of the Azure table used for storing job summaries.
        /// </summary>
        public string JobSummaryTableName { get; set; } = Defaults.JobSummaryTableName;
    }
}

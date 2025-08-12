namespace Application.Configurations
{
    /// <summary>
    /// Represents the configuration settings for job execution in the EzLead system.
    /// </summary>
    /// <remarks>This class provides settings that control the behavior of job execution, such as the maximum
    /// allowed duration.</remarks>
    public class EzLeadSettings
    {
        /// <summary>
        /// Gets or sets the maximum time between job executions in seconds.
        /// </summary>
        public int JobExecutionInSeconds { get; set; } = 3600;

        /// <summary>
        /// Gets or set the caching location for company profiles.
        /// </summary>
        public string FileCompanyProfileDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the caching location for job profiles.
        /// </summary>
        public string FileJobProfileDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cache expiration.
        /// </summary>
        public int CacheExpirationInMinutes { get; set; } = 1440;
    }
}

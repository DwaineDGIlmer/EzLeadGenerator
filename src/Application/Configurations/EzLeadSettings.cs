using Application.Constants;

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
        /// Gets or sets the cache expiration time, in minutes.
        /// </summary>
        public int CompanyCacheExpirationInDays { get; set; } = Defaults.CompanyCacheExpirationInDays;

        /// <summary>
        /// Gets or sets the duration, in hours, for which job data is cached.
        /// </summary>
        /// <remarks>Adjust this value to control how frequently the cached job data is refreshed.  A
        /// higher value reduces the frequency of cache updates, while a lower value ensures more up-to-date
        /// data.</remarks>
        public int JobsCacheExpirationInHours { get; set; } = Defaults.JobsCacheExpirationInHours;

        /// <summary>
        /// Gets or sets the number of minutes after which a SERP API query result expires.
        /// </summary>
        public int SerpApiQueryExpirationInMinutes { get; set; } = Defaults.SerpApiQueryExpirationInMinutes;
    }
}

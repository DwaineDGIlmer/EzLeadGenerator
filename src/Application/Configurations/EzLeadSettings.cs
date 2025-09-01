using Application.Constants;
using Microsoft.Extensions.Logging;

namespace Application.Configurations
{
    /// <summary>
    /// Represents the configuration settings for job execution in the EzLead system.
    /// </summary>
    /// <remarks>This class provides settings that control the behavior of job execution, such as the maximum
    /// allowed duration.</remarks>
    public sealed class EzLeadSettings
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

        /// <summary>
        /// Gets or sets a value indicating whether logging is enabled.
        /// </summary>
        public bool LoggingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the logging level for the application.
        /// </summary>
        /// <remarks>The logging level determines the minimum severity of log messages that will be
        /// recorded. For example, setting this property to <see cref="LogLevel.Warning"/> will ensure that only
        /// warnings, errors, and critical messages are logged, while informational and debug messages are
        /// ignored.</remarks>
        public LogLevel LoggingLevel { get; set; } = LogLevel.Warning;

        /// <summary>
        /// Gets or sets the unique identifier for the application.
        /// </summary>
        public string ApplicationId { get; set; } = Defaults.EzLeadGenerator;

        /// <summary>
        /// Gets or sets the unique identifier for the component.
        /// </summary>
        public string ComponentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the blob used for logging.
        /// </summary>
        public string LoggingBlobName { get; set; } = Defaults.LoggingBlobName;

        /// <summary>
        /// Gets or sets the prefix name of the blob used for logging.
        /// </summary>
        public string LoggingPrefix { get; set; } = Defaults.LoggingPrefix;

        /// <summary>
        /// Gets or sets the name of the container used for logging.
        /// </summary>
        public string LoggingContainerName { get; set; } = Defaults.LoggingContainerName;

        /// <summary>
        /// Local cache location.
        /// </summary>
        public string LoggingLocalCache { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the environment in which the application is running.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EzLeadSettings"/> class.
        /// </summary>
        /// <remarks>The constructor sets the <see cref="Environment"/> property to the value of the
        /// "ASPNETCORE_ENVIRONMENT" environment variable, or defaults to "Production" if the variable is not
        /// set.</remarks>
        public EzLeadSettings()
        {
            Environment = System.Environment.GetEnvironmentVariable(Defaults.AspNetCoreEnvironment) ?? "Production";
        }
    }
}

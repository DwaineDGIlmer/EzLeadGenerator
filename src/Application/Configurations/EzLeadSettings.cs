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
    }
}

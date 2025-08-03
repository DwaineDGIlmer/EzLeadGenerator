using Application.Configurations;
using Application.Contracts;
using Microsoft.Extensions.Options;

namespace WebApp.Middleware
{
    /// <summary>
    /// Middleware that performs periodic updates to job sources and company profiles during HTTP request processing.
    /// </summary>
    /// <remarks>This middleware checks if the last update execution occurred more than the configured
    /// interval in seconds. If so, it triggers updates to job sources and company profiles using the associated
    /// services. After performing the updates, it passes control to the next middleware in the pipeline.</remarks>
    public class JobServicesMiddleware
    {
        private readonly int _jobExecutionInSeconds;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly RequestDelegate _next;
        private readonly IJobSourceService _jobSourceService;
        private readonly ILogger<JobServicesMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobServicesMiddleware"/> class, which processes job-related
        /// services within the middleware pipeline.
        /// </summary>
        /// <param name="next">The next middleware delegate in the pipeline. Cannot be <see langword="null"/>.</param>
        /// <param name="options">The configuration settings for the middleware, encapsulated in an <see cref="IOptions{TOptions}"/> object.
        /// Cannot be <see langword="null"/>.</param>
        /// <param name="jobSourceService">The service responsible for providing job-related data and operations. Cannot be <see langword="null"/>.</param>
        /// <param name="logger">The logger instance used for logging middleware activity. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="next"/>, <paramref name="jobSourceService"/>, or <paramref name="logger"/> is <see
        /// langword="null"/>.</exception>
        public JobServicesMiddleware(
        RequestDelegate next,
        IOptions<EzLeadSettings> options,
        IJobSourceService jobSourceService,
        ILogger<JobServicesMiddleware> logger)
        {
            _jobExecutionInSeconds = ValidateJobExecutionInSeconds(options);
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _jobSourceService = jobSourceService ?? throw new ArgumentNullException(nameof(jobSourceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets or sets the timestamp of the most recent execution.
        /// </summary>
        public static DateTime LastExecution { get; set; } = default;

        /// <summary>
        /// Processes the incoming HTTP request and manages periodic job execution for updating job sources and company
        /// profiles.
        /// </summary>
        /// <remarks>This middleware ensures that job sources and company profiles are updated
        /// periodically based on a configured interval. If the interval has elapsed since the last execution, the
        /// update process is triggered asynchronously. The method also ensures thread-safe access to the update process
        /// using a semaphore.</remarks>
        /// <param name="context">The <see cref="HttpContext"/> representing the current HTTP request. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                _logger.LogError("HttpContext is null in JobServicesMiddleware.");
                return;
            }

            _logger.LogInformation("JobServicesMiddleware invoked at {Time}", DateTime.UtcNow);

            await _semaphore.WaitAsync();
            try
            {
                if (LastExecution == default)
                {
                    _logger.LogInformation("Initial updating job sources and company profiles at {Time}", DateTime.UtcNow);
                    await UpdateSourceAsync();
                }
                else if (DateTime.UtcNow - LastExecution > TimeSpan.FromSeconds(_jobExecutionInSeconds))
                {
                    _logger.LogInformation("Updating job sources and company profiles at {Time}", DateTime.UtcNow);
                    _ = Task.Run(UpdateSourceAsync).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            _logger.LogError(t.Exception, "Exception occurred while updating job sources and company profiles.");
                        }
                    });
                }
            }
            finally
            {
                LastExecution = DateTime.UtcNow;
                _semaphore.Release();
            }
            await _next(context);
        }

        private async Task UpdateSourceAsync()
        {
            await _jobSourceService.UpdateJobSourceAsync();
            await _jobSourceService.UpdateCompanyProfilesAsync();
        }

        private static int ValidateJobExecutionInSeconds(IOptions<EzLeadSettings> options)
        {
            if (options == null || options.Value == null || options.Value.JobExecutionInSeconds <= 0)
            {
                throw new ArgumentNullException(nameof(options), "Job execution interval must be configured with a positive value.");
            }
            return options.Value.JobExecutionInSeconds;
        }
    }
}

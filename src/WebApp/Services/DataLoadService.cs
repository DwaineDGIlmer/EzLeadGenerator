using Application.Contracts;

namespace WebApp.Services
{
    /// <summary>
    /// Provides functionality for configuring and initializing application services related to job sources.
    /// </summary>
    /// <remarks>The <see cref="DataLoadService"/> class contains methods for setting up and updating job
    /// source services within an application's request pipeline. It ensures that required services are registered and
    /// initialized properly.</remarks>
    public static class DataLoadService
    {

        /// <summary>
        /// Configures the application to use the job source service.
        /// </summary>
        /// <remarks>This method ensures that the job source service is registered and performs initial
        /// updates to the job source and company profiles. It throws an exception if the service is not
        /// available.</remarks>
        /// <param name="app">The application builder used to configure the application's request pipeline.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance for further configuration.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the job source service is not registered in the application's service collection.</exception>
        public static async Task LoadAppSourceService(IApplicationBuilder app)
        {
            var job = app.ApplicationServices.GetService<IJobSourceService>() ?? throw new InvalidOperationException("Job source service is not registered.");
            await job.UpdateJobSourceAsync();
            await job.UpdateCompanyProfilesAsync();
        }
    }
}

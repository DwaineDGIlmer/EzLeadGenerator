using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    /// <summary>
    /// Represents the model for the Privacy page in an ASP.NET Core Razor Pages application.
    /// </summary>
    /// <remarks>This class is used to handle the data and behavior for the Privacy page. It includes a method
    /// to handle GET requests and supports logging through dependency injection.</remarks>
    /// <remarks>
    /// Initializes a new instance of the <see cref="PrivacyModel"/> class.
    /// </remarks>
    /// <param name="logger">The logger instance used to log messages related to the <see cref="PrivacyModel"/>.</param>
    public class PrivacyModel(ILogger<PrivacyModel> logger) : PageModel
    {
        /// <summary>
        /// The logger instance used for logging information and errors.
        /// </summary>
        private readonly ILogger<PrivacyModel> _logger = logger;

        /// <summary>
        /// Handles GET requests for the page.
        /// </summary>
        /// <remarks>This method is invoked when the page is accessed via an HTTP GET request. Override
        /// this method in a derived class to implement custom logic for handling GET requests.</remarks>
        public void OnGet()
        {
            _logger.LogInformation("Privacy page accessed.");
        }
    }

}

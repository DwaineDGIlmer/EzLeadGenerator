using System.Diagnostics;

namespace WebApp.Pages
{
    /// <summary>
    /// Represents the model for handling error pages in an ASP.NET Core application.
    /// </summary>
    /// <remarks>This model is used to display error information, including a unique request identifier that
    /// can help in diagnosing issues. It is designed to work with Razor Pages and includes logging capabilities for
    /// error tracking.</remarks>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    sealed public class ErrorModel : PageModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the current request.
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether the request ID should be shown.
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Represents an error with details about the error message and associated code.
        /// </summary>
        public ErrorModel() { }

        /// <summary>
        /// Handles GET requests and initializes the request identifier.
        /// </summary>
        /// <remarks>This method sets the <see cref="RequestId"/> property to the current activity ID if
        /// available,  or to the HTTP context trace identifier as a fallback. This can be used for tracking and logging
        /// purposes.</remarks>
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }

}

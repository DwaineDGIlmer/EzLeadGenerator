namespace WebApp.Pages
{
    /// <summary>
    /// Represents the model for the "About" page in an ASP.NET Core Razor Pages application.
    /// </summary>
    /// <remarks>This class is used to handle the data and logic for the "About" page.  The <see
    /// cref="OnGet"/> method is invoked when a GET request is made to the page.</remarks>
    public sealed class AboutModel : PageModel
    {
        /// <summary>
        /// Handles GET requests for the page.
        /// </summary>
        /// <remarks>This method is invoked when the page is accessed via an HTTP GET request. Override
        /// this method in a derived class to implement custom logic for handling GET requests.</remarks>
        public void OnGet()
        {
        }
    }
}

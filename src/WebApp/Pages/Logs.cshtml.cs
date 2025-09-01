using Application.Services;

namespace WebApp.Pages;

/// <summary>
/// Represents the page model for displaying and managing log folders.
/// </summary>
/// <remarks>This model is used to interact with the log storage service and retrieve a list of available log
/// folders. It is designed to be used in Razor Pages for displaying log-related data.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="LogsModel"/> class with the specified log service.
/// </remarks>
/// <param name="logService">The service used to read log data from the underlying storage.</param>
public sealed class LogsModel(LogBlobReaderService logService) : PageModel
{
    private readonly LogBlobReaderService _logService = logService;

    /// <summary>
    /// Gets or sets the collection of folder paths.
    /// </summary>
    public List<string> Folders { get; set; } = [];

    /// <summary>
    /// Handles the GET request for the page and retrieves the list of folders.
    /// </summary>
    /// <remarks>This method asynchronously fetches the folder data using the log service and populates the
    /// <see cref="Folders"/> property.</remarks>
    /// <returns></returns>
    public async Task OnGetAsync()
    {
        Folders = await _logService.ListFoldersAsync();
    }
}

using Application.Services;

namespace WebApp.Pages;

/// <summary>
/// Represents the model for a folder page, providing functionality to retrieve and display log files within a specified
/// folder.
/// </summary>
/// <remarks>This model is used to interact with the folder page, allowing users to view a list of log files in a
/// given folder. The <see cref="OnGetAsync(string)"/> method is called to populate the <see cref="Files"/> property
/// with the log files from the specified folder.</remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="FolderModel"/> class.
/// </remarks>
/// <param name="logService">The service used to read log data from a blob storage. This parameter cannot be <see langword="null"/>.</param>
public sealed class FolderModel(LogBlobReaderService logService) : PageModel
{
    private readonly LogBlobReaderService _logService = logService;

    /// <summary>
    /// Gets or sets the collection of file paths.
    /// </summary>
    public List<string> Files { get; set; } = [];

    /// <summary>
    /// Gets or sets the folder path associated with the operation.
    /// </summary>
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// Handles the GET request for the page and retrieves the list of log files in the specified folder.
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public async Task OnGetAsync(string folder)
    {
        Folder = folder;
        Files = await _logService.ListLogFilesAsync(folder);
    }
}
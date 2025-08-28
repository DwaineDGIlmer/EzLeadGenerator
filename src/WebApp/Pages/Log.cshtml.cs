using Application.Services;

namespace WebApp.Pages;

/// <summary>
/// Represents the model for handling log file operations within a specific folder and file context.
/// </summary>
/// <remarks>This class is used in conjunction with the Razor PageModel to manage log file data. It provides
/// properties for specifying the folder and file being accessed, as well as the log event data retrieved from the log
/// service. The <see cref="OnGetAsync"/> method is used to initialize the model with data from the specified folder and
/// file.</remarks>
/// <param name="logService"></param>
public class LogModel(LogBlobReaderService logService) : PageModel
{
    private readonly LogBlobReaderService _logService = logService;

    /// <summary>
    /// Gets or sets the folder path associated with the operation.
    /// </summary>
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the log file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the log event associated with the current operation.
    /// </summary>
    public LogEvent? LogEvent { get; set; }

    /// <summary>
    /// Handles the GET request by retrieving the log event for the specified folder and file.
    /// </summary>
    /// <remarks>This method sets the <see cref="Folder"/> and <see cref="FileName"/> properties to the provided
    /// values  and retrieves the log event using the specified folder and file. The retrieved log event is stored  in
    /// the <see cref="LogEvent"/> property.</remarks>
    /// <param name="folder">The name of the folder containing the log file. Cannot be null or empty.</param>
    /// <param name="file">The name of the log file to retrieve. Cannot be null or empty.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task OnGetAsync(string folder, string file)
    {
        Folder = folder;
        FileName = file;
        LogEvent = await _logService.ReadLogAsync(folder, file);
    }
}
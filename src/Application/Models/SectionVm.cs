namespace Application.Models;

/// <summary>
/// Represents a section containing a title and a collection of items.
/// </summary>
/// <remarks>This class is typically used to group related items under a common title.</remarks>
sealed public class SectionVm
{
    /// <summary>
    /// Gets or sets the title associated with the current object.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of result items.
    /// </summary>
    public List<OrganicResultItem> Items { get; set; } = new();
}

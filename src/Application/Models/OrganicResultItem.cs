namespace Application.Models;


/// <summary>
/// Represents a result item with metadata and content details, such as title, URL, source, and tags.
/// </summary>
/// <remarks>This class is commonly used to encapsulate information about a search result or content item. It
/// includes properties for identifying the item (e.g., <see cref="Title"/> and <see cref="Url"/>), categorizing it
/// (e.g., <see cref="Type"/> and <see cref="Tags"/>), and providing additional context (e.g., <see cref="Source"/>,
/// <see cref="Date"/>, and <see cref="Location"/>).</remarks>
public class OrganicResultItem
{
    /// <summary>
    /// Gets or sets the title associated with the current object.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL associated with the current instance.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location associated with the object.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source identifier associated with the current operation or entity.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the URL or hyperlink text to be displayed.
    /// </summary>
    public string? DisplayedLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date associated with the current instance.
    /// </summary>
    public string? Date { get; set; }

    /// <summary>
    /// Gets or sets a brief excerpt or segment of text.
    /// </summary>
    public string Snippet { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the entity or object represented by this instance.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <see cref="MustInclude"/> instance that must be included in the operation.
    /// </summary>
    public MustInclude? MustInclude { get; set; }

    /// <summary>
    /// Gets or sets the collection of tags associated with the current object.
    /// </summary>
    public string[] Tags { get; set; } = [];
}

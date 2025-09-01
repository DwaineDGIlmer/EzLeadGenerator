using System.Text.Json.Serialization;

namespace Application.Models;

/// <summary>
/// Represents an organic search result.
/// </summary>
public sealed class OrganicResult
{
    /// <summary>
    /// Gets or sets the position index within a collection.
    /// </summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the title of the item.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hyperlink associated with the object.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the redirect link associated with the object.
    /// </summary>
    [JsonPropertyName("redirect_link")]
    public string RedirectLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the displayed link associated with the object.
    /// </summary>
    [JsonPropertyName("displayed_link")]
    public string DisplayedLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the thumbnail image.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string Thumbnail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date associated with the current object.
    /// </summary>
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the snippet of text associated with the JSON property.
    /// </summary>
    [JsonPropertyName("snippet")]
    public string Snippet { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of words that are highlighted in the snippet.
    /// </summary>
    [JsonPropertyName("snippet_highlighted_words")]
    public List<string> SnippetHighlightedWords { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of words that are highlighted in the snippet.
    /// </summary>
    [JsonPropertyName("rich_snippet")]
    public TopInfo RichSnippet { get; set; } = new();

    /// <summary>
    /// Gets or sets the string value that specifies the items or keywords that must be included.
    /// </summary>
    [JsonPropertyName("must_include")]
    public MustInclude? MustInclude { get; set; }

    /// <summary>
    /// Gets or sets the source of the data or content.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Represents an entity containing a word and its associated link.
/// </summary>
public sealed class MustInclude
{
    /// <summary>
    /// Gets or sets the word associated with this instance.
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hyperlink associated with the object.
    /// </summary>
    public string Link { get; set; } = string.Empty;
}

/// <summary>
/// Represents information about the top-level details.
/// </summary>
/// <remarks>This class provides access to the top-level details through the <see cref="Top"/> property.</remarks>
public sealed class TopInfo
{
    /// <summary>
    /// Gets or sets the details associated with the top-level configuration or entity.
    /// </summary>
    public TopDetails Top { get; set; } = new();
}

/// <summary>
/// Represents a collection of file extensions associated with a specific context or operation.
/// </summary>
/// <remarks>This class provides a property to store and retrieve a list of file extensions.  It can be used to
/// define or filter supported file types in various scenarios, such as file uploads or processing.</remarks>
public sealed class TopDetails
{
    /// <summary>
    /// Gets or sets the list of file extensions associated with this instance.
    /// </summary>
    public List<string> Extensions { get; set; } = [];
}

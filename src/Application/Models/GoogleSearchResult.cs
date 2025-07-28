using System.Text.Json.Serialization;

namespace Application.Models;

/// <summary>
/// Represents the search result data retrieved from a search engine.
/// </summary>
public class GoogleSearchResult
{
    /// <summary>
    /// Metadata about the search, including ID, status, and timing information.
    /// </summary>
    [JsonPropertyName("search_metadata")]
    public SearchMetadata SearchMetadata { get; set; } = new();

    /// <summary>
    /// Parameters used for the search query.
    /// </summary>
    [JsonPropertyName("search_parameters")]
    public SearchParameters SearchParameters { get; set; } = new();

    /// <summary>
    /// Information about the search results, such as total results and query details.
    /// </summary>
    [JsonPropertyName("search_information")]
    public SearchInformation SearchInformation { get; set; } = new();

    /// <summary>
    /// List of organic search results.
    /// </summary>
    [JsonPropertyName("organic_results")]
    public List<OrganicResult> OrganicResults { get; set; } = [];

    /// <summary>
    /// Related searches based on the query.
    /// </summary>
    [JsonPropertyName("related_searches")]
    public List<RelatedSearch> RelatedSearches { get; set; } = [];

    /// <summary>
    /// Pagination details for the search results.
    /// </summary>
    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; set; } = new();

    /// <summary>
    /// Pagination details specific to the SerpAPI.
    /// </summary>
    [JsonPropertyName("serpapi_pagination")]
    public SerpApiPagination SerpApiPagination { get; set; } = new();
}

/// <summary>
/// Represents metadata about the search.
/// </summary>
public class SearchMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current status of the operation.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON endpoint URL.
    /// </summary>
    [JsonPropertyName("json_endpoint")]
    public string JsonEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation timestamp of the entity.
    /// </summary>
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp indicating when the processing was completed.
    /// </summary>
    [JsonPropertyName("processed_at")]
    public string ProcessedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the Google resource.
    /// </summary>
    [JsonPropertyName("google_url")]
    public string GoogleUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw HTML file content as a string.
    /// </summary>
    [JsonPropertyName("raw_html_file")]
    public string RawHtmlFile { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total time taken for the operation, in seconds.
    /// </summary>
    [JsonPropertyName("total_time_taken")]
    public double TotalTimeTaken { get; set; }
}

/// <summary>
/// Represents the parameters used for the search query.
/// </summary>
public class SearchParameters
{
    /// <summary>
    /// Gets or sets the engine type identifier.
    /// </summary>
    [JsonPropertyName("engine")]
    public string Engine { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the search query string.
    /// </summary>
    [JsonPropertyName("q")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested location as a string.
    /// </summary>
    [JsonPropertyName("location_requested")]
    public string LocationRequested { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location used in the current context.
    /// </summary>
    [JsonPropertyName("location_used")]
    public string LocationUsed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Google domain associated with the user or service.
    /// </summary>
    [JsonPropertyName("google_domain")]
    public string GoogleDomain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the language code used for localization.
    /// </summary>
    [JsonPropertyName("hl")]
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country code associated with the entity.
    /// </summary>
    [JsonPropertyName("gl")]
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SafeSearch setting for the search query.
    /// </summary>
    [JsonPropertyName("safe")]
    public string SafeSearch { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the starting index for the operation.
    /// </summary>
    [JsonPropertyName("start")]
    public int Start { get; set; }

    /// <summary>
    /// Gets or sets the number of results as a string.
    /// </summary>
    [JsonPropertyName("num")]
    public string NumResults { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device identifier.
    /// </summary>
    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;
}

/// <summary>
/// Represents information about the search results.
/// </summary>
public class SearchInformation
{
    /// <summary>
    /// Gets or sets the state of the organic search results.
    /// </summary>
    [JsonPropertyName("organic_results_state")]
    public string OrganicResultsState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the query string that is displayed to the user.
    /// </summary>
    [JsonPropertyName("query_displayed")]
    public string QueryDisplayed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total number of results returned by the query.
    /// </summary>
    [JsonPropertyName("total_results")]
    public long TotalResults { get; set; }

    /// <summary>
    /// Gets or sets the current page number in a paginated list.
    /// </summary>
    [JsonPropertyName("page_number")]
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the time taken for the operation, formatted for display purposes.
    /// </summary>
    [JsonPropertyName("time_taken_displayed")]
    public double TimeTakenDisplayed { get; set; }
}

/// <summary>
/// Represents an organic search result.
/// </summary>
public class OrganicResult
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
}

/// <summary>
/// Represents a related search.
/// </summary>
public class RelatedSearch
{
    /// <summary>
    /// Gets or sets the query string used for searching or filtering data.
    /// </summary>
    [JsonPropertyName("query")]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hyperlink associated with the object.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;
}

/// <summary>
/// Represents pagination details.
/// </summary>
public class Pagination
{
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    [JsonPropertyName("current")]
    public int Current { get; set; }

    /// <summary>
    /// Gets or sets the URL of the previous page in a paginated list.
    /// </summary>
    [JsonPropertyName("previous")]
    public string Previous { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL for the next page of results in a paginated response.
    /// </summary>
    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a dictionary containing additional pages, where the key is the page identifier and the value is the
    /// page content.
    /// </summary>
    [JsonPropertyName("other_pages")]
    public Dictionary<string, string> OtherPages { get; set; } = [];
}

/// <summary>
/// Represents SerpAPI-specific pagination details.
/// </summary>
public class SerpApiPagination
{
    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    [JsonPropertyName("current")]
    public int Current { get; set; }

    /// <summary>
    /// Gets or sets the URL of the previous page in a paginated list.
    /// </summary>
    [JsonPropertyName("previous_link")]
    public string PreviousLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL for the next page of results in a paginated response.
    /// </summary>
    [JsonPropertyName("next_link")]
    public string NextLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a dictionary containing additional pages, where the key is the page identifier and the value is the
    /// page content.
    /// </summary>
    [JsonPropertyName("other_pages")]
    public Dictionary<string, string> OtherPages { get; set; } = [];
}
using System.Text.Json.Serialization;

namespace Application.Models;

/// <summary>
/// Represents the results of a Google Jobs search, including metadata, parameters, filters, job listings, and
/// pagination details.
/// </summary>
/// <remarks>This class encapsulates the data returned from a Google Jobs search query. It includes various
/// components such as search metadata, parameters used for the search, applied filters, the list of job results, and
/// pagination information.</remarks>
public class GoogleJobsResult
{
    /// <summary>
    /// Gets or sets the metadata associated with a search operation.
    /// </summary>
    [JsonPropertyName("search_metadata")]
    public JobsSearchMetadata SearchMetadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the parameters used for configuring search operations.
    /// </summary>
    [JsonPropertyName("search_parameters")]
    public SearchParameters SearchParameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of filters applied to the data.
    /// </summary>
    [JsonPropertyName("filters")]
    public List<Filter> Filters { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of job results.
    /// </summary>
    [JsonPropertyName("jobs_results")]
    public List<JobResult> JobsResults { get; set; } = [];

    /// <summary>
    /// Gets or sets the pagination details for Serpapi results.
    /// </summary>
    [JsonPropertyName("serpapi_pagination")]
    public SerpapiPagination SerpapiPagination { get; set; } = new();
}

/// <summary>
/// Represents metadata associated with a search operation, including identifiers, status, and timing information.
/// </summary>
/// <remarks>This class provides properties to access various details about a search operation, such as its unique
/// identifier, status, and timing information. It also includes URLs for accessing related resources and the raw HTML
/// content of the search result.</remarks>
public class JobsSearchMetadata
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
    /// Gets or sets the timestamp when the entity was last updated.
    /// </summary>  
    [JsonPropertyName("processed_at")]
    public string ProcessedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL for accessing Google Jobs.
    /// </summary>
    [JsonPropertyName("google_jobs_url")]
    public string GoogleJobsUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw HTML content of the file.
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
/// Represents a set of parameters used for configuring a request or operation.
/// </summary>
public class Parameters
{
    /// <summary>
    /// Gets or sets the Universal Data System (UDS) identifier.
    /// </summary>
    [JsonPropertyName("uds")]
    public string Uds { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the property Q.
    /// </summary>
    [JsonPropertyName("q")]
    public string Q { get; set; } = string.Empty;
}

/// <summary>
/// Represents an option with associated parameters and links.
/// </summary>
/// <remarks>This class encapsulates the details of an option, including its name, parameters, and related
/// links.</remarks>
public class Option
{
    /// <summary>
    /// Gets or sets the name associated with the current instance.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameters used for configuring the operation.
    /// </summary>
    [JsonPropertyName("parameters")]
    public Parameters Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the hyperlink associated with the object.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL link to the Serpapi service.
    /// </summary>
    [JsonPropertyName("serpapi_link")]
    public string SerpapiLink { get; set; } = string.Empty;
}

/// <summary>
/// Represents a filter with associated parameters and options for a search operation.
/// </summary>
/// <remarks>This class encapsulates the details of a filter, including its name, parameters, and options, as well
/// as links related to the filter's operation. It is typically used to configure and execute search queries with
/// specific criteria.</remarks>
public class Filter
{
    /// <summary>
    /// Gets or sets the name associated with the current instance.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameters used for configuring the operation.
    /// </summary>
    [JsonPropertyName("parameters")]
    public Parameters Parameters { get; set; } = new();

    /// <summary>
    /// Gets or sets the hyperlink associated with the object.
    /// </summary>
    [JsonPropertyName("link")]
    public string Link { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL link to the Serpapi service.
    /// </summary>
    [JsonPropertyName("serpapi_link")]
    public string SerpapiLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of options available for selection.
    /// </summary>
    [JsonPropertyName("options")]
    public List<Option> Options { get; set; } = [];
}

/// <summary>
/// Represents a collection of detected extensions related to employment benefits and scheduling.
/// </summary>
/// <remarks>This class provides properties to store information about the posting date, schedule type, and
/// various employment benefits such as health insurance, dental coverage, and paid time off. Each property can be
/// individually set or retrieved to reflect the current state of these extensions.</remarks>
public class DetectedExtensions
{
    /// <summary>
    /// Gets or sets the timestamp indicating when the post was created.
    /// </summary>
    [JsonPropertyName("posted_at")]
    public string PostedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of schedule.
    /// </summary>
    [JsonPropertyName("schedule_type")]
    public string ScheduleType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the individual has health insurance coverage.
    /// </summary>
    [JsonPropertyName("health_insurance")]
    public bool? HealthInsurance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether dental coverage is included.
    /// </summary>
    [JsonPropertyName("dental_coverage")]
    public bool? DentalCoverage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the employee is eligible for paid time off.
    /// </summary>
    [JsonPropertyName("paid_time_off")]
    public bool? PaidTimeOff { get; set; }
}

/// <summary>
/// Represents a highlight of a job, including a title and a list of items describing the highlight.
/// </summary>
public class JobHighlight
{
    /// <summary>
    /// Gets or sets the title of the item.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of items.
    /// </summary>
    [JsonPropertyName("items")]
    public List<string> Items { get; set; } = [];
}

/// <summary>
/// Represents an option with a title and a link, typically used for navigation or selection purposes.
/// </summary>
public class ApplyOption
{
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
}

/// <summary>
/// Represents the result of a job search, containing details about a specific job listing.
/// </summary>
/// <remarks>This class encapsulates various properties of a job listing, such as the job title, company name,
/// location, and additional metadata. It is used to store and transfer job-related information retrieved from a job
/// search operation.</remarks>
public class JobResult
{
    /// <summary>
    /// Gets or sets the title of the item.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the company.
    /// </summary>
    [JsonPropertyName("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location associated with the current instance.
    /// </summary>
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the route or intermediary through which the request is sent.
    /// </summary>
    [JsonPropertyName("via")]
    public string Via { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL used for sharing content.
    /// </summary>
    [JsonPropertyName("share_link")]
    public string ShareLink { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of extensions.
    /// </summary>
    [JsonPropertyName("extensions")]
    public List<string> Extensions { get; set; } = [];

    /// <summary>
    /// Gets or sets the detected extensions.
    /// </summary>
    [JsonPropertyName("detected_extensions")]
    public DetectedExtensions DetectedExtensions { get; set; } = new();

    /// <summary>
    /// Gets or sets the description text.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of job highlights.
    /// </summary>
    [JsonPropertyName("job_highlights")]
    public List<JobHighlight> JobHighlights { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of options to be applied.
    /// </summary>
    [JsonPropertyName("apply_options")]
    public List<ApplyOption> ApplyOptions { get; set; } = [];

    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Represents pagination information for navigating through search results in the Serpapi API.
/// </summary>
/// <remarks>This class provides properties to access tokens and URLs for fetching the next page of
/// results.</remarks>
public class SerpapiPagination
{
    /// <summary>
    /// Gets or sets the token used to retrieve the next page of results in a paginated query.
    /// </summary>
    [JsonPropertyName("next_page_token")]
    public string NextPageToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the next item in a sequence.
    /// </summary>
    [JsonPropertyName("next")]
    public string Next { get; set; } = string.Empty;
}
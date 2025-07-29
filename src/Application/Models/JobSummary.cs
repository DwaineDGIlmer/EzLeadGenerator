using Application.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Application.Models;

/// <summary>
/// Represents a summary of a job posting, including company details, job title, location,
/// </summary>
public class JobSummary
{
    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the date and time when the post was created.
    /// </summary>
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the unique identifier for the job, typically generated as a hash of the company, title, and date.
    /// </summary>
    [Required]
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// Represents a summary of a job posting, including company details, job title, location,
    /// description, and AI analysis.
    /// </summary>
    [Required]
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the company offering the job.
    /// </summary>
    [Required]
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the talent agency.
    /// </summary>
    public string HiringAgency { get; set; } = string.Empty;

    /// <summary>
    /// Represents a summary of a job posting, including company details, job title, location,
    /// description, and AI analysis.
    /// </summary>
    [Required]
    public string JobTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the location of the job.
    /// This can be a city, state, or country, and is optional.
    /// </summary>
    [Required]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the job posting.
    /// </summary>
    public string JobDescription { get; set; } = string.Empty;

    /// <summary>
    /// Represents the AI analysis of the job posting, including the division it belongs to,
    /// </summary>
    public string Division { get; set; } = string.Empty;

    /// <summary>
    /// Represents the AI analysis of the job posting, including the division it belongs to,
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// Represents the AI analysis of the job posting, including the division it belongs to,
    /// </summary>
    public string Reasoning { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the job was posted.
    /// </summary>
    public string SourceLink { get; set; } = string.Empty;

    /// <summary>
    /// Represents the source of the job posting, such as the platform or company that posted it
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of job highlights.
    /// </summary>
    public IList<JobHighlight> JobHighlights { get; set; } = [];

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the post was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobSummary"/> class.
    /// </summary>
    public JobSummary() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="JobSummary"/> class using the specified job result.
    /// </summary>
    /// <remarks>This constructor extracts relevant job information from the provided <paramref
    /// name="jobResult"/> and initializes the properties of the <see cref="JobSummary"/> instance. The <see
    /// cref="CompanyId"/> is sanitized for file system compatibility.</remarks>
    /// <param name="jobResult">The job result containing details about the job, such as company name, job title, location, and description.</param>
    public JobSummary(JobResult jobResult)
    {
        CompanyName = jobResult.CompanyName;
        CompanyId = jobResult.CompanyName.FileSystemName();
        JobTitle = jobResult.Title;
        Location = jobResult.Location;
        JobDescription = jobResult.Description;
        JobHighlights = jobResult.JobHighlights ?? [];
        HiringAgency = jobResult.Via;
        SourceLink = jobResult.ShareLink;
        SourceName = "Google Jobs";
        JobId = jobResult.JobId;
    }

    /// <summary>
    /// Returns a string representation of the current object, including key properties such as JobTitle and PostedDate.
    /// </summary>
    /// <returns>A string that represents the current object, formatted to include the JobTitle and PostedDate properties.</returns>
    public override string ToString()
    {
        return $"JobTitle: {JobTitle}, PostedDate: {PostedDate}, ...";
    }
}
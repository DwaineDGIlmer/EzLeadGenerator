using Application.Services;
using Core.Helpers;

namespace Application.Models;

/// <summary>
/// Represents a company's profile including metadata, organizational structure,
/// and analysis used to support job and department inference.
/// </summary>
public class CompanyProfile
{
    /// <summary>
    /// A unique identifier for the company, used for routing and storage (e.g., "pwc").
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// A unique identifier for the company, used for routing and storage (e.g., "pwc").
    /// </summary>
    public string CompanyId { get; set; } = string.Empty;

    /// <summary>
    /// The full legal or brand name of the company (e.g., "PricewaterhouseCoopers").
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL link associated with the company.
    /// </summary>
    public string Link { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the domain.
    /// </summary>
    public string DomainName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the results of the hierarchy analysis.
    /// </summary>
    public HierarchyResults HierarchyResults { get; set; } = new();

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Timestamp of the most recent job data pull or analysis for this company.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyProfile"/> class.
    /// </summary>
    public CompanyProfile() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyProfile"/> class with the specified job result and hierarchy
    /// results.
    /// </summary>
    /// <param name="jobSummary">The job result containing the company name used to initialize the profile.</param>
    /// <param name="hierarchyResults">The hierarchy results to associate with the company profile. If null, a new instance of <see
    /// cref="HierarchyResults"/> is created.</param>
    public CompanyProfile(JobSummary jobSummary, HierarchyResults hierarchyResults)
    {
        CompanyName = jobSummary.CompanyName;
        CompanyId = jobSummary.CompanyName.FileSystemName();
        HierarchyResults = hierarchyResults ?? new HierarchyResults();
    }
}

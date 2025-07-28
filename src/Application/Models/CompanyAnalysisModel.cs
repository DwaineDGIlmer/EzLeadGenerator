namespace Application.Models
{
    /// <summary>
    /// Represents a view model for analyzing a company's structure and performance.
    /// </summary>
    /// <remarks>This view model includes details such as the company name, division, confidence level in the
    /// analysis, reasoning behind the analysis, organizational structure, and team leads. It is intended to be used in
    /// scenarios where a detailed overview of a company's internal analysis is required.</remarks>
    public class CompanyAnalysisModel
    {
        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// The location of the company headquarters or main office.
        /// </summary>
        public string CompanyLocation { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the division name associated with the entity.
        /// </summary>
        public string Division { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confidence level as a percentage.
        /// </summary>
        public int Confidence { get; set; }

        /// <summary>
        /// Gets or sets the reasoning behind the decision or action.
        /// </summary>
        public string Reasoning { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the organizational structure as a string representation.
        /// </summary>
        public string OrgStructure { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the names of the team leads.
        /// </summary>
        public string TeamLeads { get; set; } = string.Empty;
    }
}

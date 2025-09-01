namespace Application.Models
{
    /// <summary>
    /// Represents an inference about a division, including the reasoning and confidence level.
    /// </summary>
    /// <remarks>This class is used to encapsulate information about a division inference, which includes the
    /// division name, the reasoning behind the inference, and a confidence level indicating the certainty of the
    /// inference.</remarks>
    public sealed class DivisionInference
    {
        /// <summary>
        /// Gets or sets the division name associated with the entity.
        /// </summary>
        public string Division { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the reasoning behind the decision or action.
        /// </summary>
        public string Reasoning { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the confidence level as a percentage.
        /// </summary>
        public int Confidence { get; set; }
    }
}
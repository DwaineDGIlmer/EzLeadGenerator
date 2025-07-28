using Application.Models;

namespace Application.Contracts
{
    /// <summary>
    /// Provides functionality to infer division information from a company profile.
    /// </summary>
    public interface IDivisionInferenceService
    {
        /// <summary>
        /// Infers the division of a company based on the provided company profile.
        /// </summary>
        /// <param name="profile">The company profile containing information used for division inference. Cannot be null.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="DivisionInference"/>
        /// object representing the inferred division, or <see langword="null"/> if the division cannot be determined.</returns>
        Task<DivisionInference?> InferDivisionFromProfileAsync(CompanyProfile profile);
    }

}

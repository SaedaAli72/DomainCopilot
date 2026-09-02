

using DomainCopilot.Application.Agents.Contracts;

namespace DomainCopilot.Application.Agents.Interfaces
{
    public interface IResponseDrafterAgent
    {
        Task<DraftedResponse> DraftAsync(EligibilityResult eligibility, ProcedureResult procedure, CancellationToken cancellationToken = default);

    }
}



using DomainCopilot.Application.Agents.Contracts;

namespace DomainCopilot.Application.Agents.Interfaces
{
    public interface IEligibilityIdentifierAgent
    {
        Task<EligibilityResult> AnalyzeAsync(string citizenSituation, Guid tenantId, CancellationToken cancellationToken = default);

    }
}

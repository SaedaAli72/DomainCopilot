

using DomainCopilot.Application.Agents.Contracts;

namespace DomainCopilot.Application.Agents.Interfaces
{
    public interface IGovernmentServiceOrchestrator
    {
        Task<DraftedResponse?> ProcessAsync(string citizenSituation, string serviceType, Guid tenantId, CancellationToken cancellationToken = default);

    }
}

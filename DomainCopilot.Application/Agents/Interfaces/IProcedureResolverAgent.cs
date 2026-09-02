

using DomainCopilot.Application.Agents.Contracts;

namespace DomainCopilot.Application.Agents.Interfaces
{
    public interface IProcedureResolverAgent
    {
        Task<ProcedureResult> ResolveAsync(string serviceType, Guid tenantId, CancellationToken cancellationToken = default);

    }
}



using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Interfaces
{
    public interface ICitizenRequestRepository
    {
        Task AddAsync(CitizenRequest request, CancellationToken cancellationToken = default);
        Task UpdateAsync(CitizenRequest request, CancellationToken cancellationToken = default);
        Task<CitizenRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<CitizenRequest>> GetPendingApprovalAsync(Guid tenantId, CancellationToken cancellationToken = default);

    }
}

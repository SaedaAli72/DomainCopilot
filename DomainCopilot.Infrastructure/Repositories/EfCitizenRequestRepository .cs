

using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enum;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Repositories
{
    public class EfCitizenRequestRepository:ICitizenRequestRepository
    {
        private readonly AppDbContext _context;

        public EfCitizenRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CitizenRequest request, CancellationToken cancellationToken = default)
        {
            _context.CitizenRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(CitizenRequest request, CancellationToken cancellationToken = default)
        {
            _context.Entry(request).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<CitizenRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.CitizenRequests.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<List<CitizenRequest>> GetPendingApprovalAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _context.CitizenRequests
                .Where(r => r.Status == RequestStatus.AwaitingApproval)
                .ToListAsync(cancellationToken);
        }
    }
}

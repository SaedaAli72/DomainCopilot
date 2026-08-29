

using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Repositories
{
    public class EfDocumentChunkRepository : IDocumentChunkRepository
    {
        private readonly AppDbContext _context;

        public EfDocumentChunkRepository(AppDbContext context)
        {
            _context = context;
        }
        public Task<List<DocumentChunk>> GetByIdsAsync(List<Guid> chunkIds, CancellationToken cancellationToken = default)
        {
           return _context.DocumentChunks
                .Where(c=>chunkIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }
    }
}



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

        public async Task AddAsync(DocumentChunk chunk, CancellationToken cancellationToken = default)
        {
            _context.DocumentChunks.Add(chunk);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<DocumentChunk>> GetByIdsAsync(List<Guid> chunkIds, CancellationToken cancellationToken = default)
        {
           return await _context.DocumentChunks
                .Where(c=>chunkIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }
    }
}

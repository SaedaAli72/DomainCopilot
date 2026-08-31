

using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence;

namespace DomainCopilot.Infrastructure.Repositories
{
    public class EfDocumentRepository : IDocumentRepository
    {
        private readonly AppDbContext _context;

        public EfDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Document document, CancellationToken cancellationToken = default)
        {
            _context.Entry(document).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

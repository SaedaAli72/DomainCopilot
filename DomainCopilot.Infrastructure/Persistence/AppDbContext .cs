

using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<CitizenRequest>CitizenRequests { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
        public DbSet<Document> Documents { get; set; }
    }
}



using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        private readonly ITenantProvider _tenantProvider;
        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<CitizenRequest>CitizenRequests { get; set; }
        public DbSet<DocumentChunk> DocumentChunks { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>().
                HasQueryFilter(d=>d.TenantId==_tenantProvider.GetCurrentTenantId());
            modelBuilder.Entity<CitizenRequest>().
                HasQueryFilter(r => r.TenantId == _tenantProvider.GetCurrentTenantId());
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new DocumentChunkConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}



using DomainCopilot.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DomainCopilot.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=DomainCopilotDb;Trusted_Connection=True;TrustServerCertificate=True;");

            var fakeTenantProvider = new StaticTenantProvider();

            return new AppDbContext(optionsBuilder.Options, fakeTenantProvider);
        }
    }
}

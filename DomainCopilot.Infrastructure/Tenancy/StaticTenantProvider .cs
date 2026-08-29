

using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Infrastructure.Tenancy
{
    public class StaticTenantProvider : ITenantProvider
    {
        // TEMPORARY: hardcoded tenant until real authentication (FR-8) is wired
        private static readonly Guid FixedTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public Guid GetCurrentTenantId()
        {
            return FixedTenantId;
        }
    }
}

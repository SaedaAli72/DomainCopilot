

namespace DomainCopilot.Application.Interfaces
{
    public interface ITenantProvider
    {
        // Returns the TenantId of whoever is making the current request
        Guid GetCurrentTenantId();
    }
}

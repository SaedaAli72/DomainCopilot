

using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DomainCopilot.Tests
{
    public class TenantIsolationTests
    {
        [Fact]
        public async Task CitizenRequests_ShouldBeIsolated_BetweenTenants()
        {
            var tenantAId = Guid.NewGuid();
            var tenantBId = Guid.NewGuid();

            //هجهز داتا بيز وهمية

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

            //هعمل اختبار فيك للاول

            var fakeTenantProvider = new FakeTenantProviderForTest(tenantAId);
           
            //obj from db
            await using var contextAsA = new AppDbContext(options, fakeTenantProvider);
            //add new citizin
            var requestA = new CitizenRequest(tenantAId, "Ahmed", "Housing Subsidy");
            contextAsA.CitizenRequests.Add(requestA);
            await contextAsA.SaveChangesAsync();

            //هعمل اختبار فيك للتانى
            var fakeTenantProviderB = new FakeTenantProviderForTest(tenantBId);
            await using var contextAsB = new AppDbContext(options, fakeTenantProviderB);

            var requestB = new CitizenRequest(tenantBId, "Sara", "Tax Exemption");
            contextAsB.CitizenRequests.Add(requestB);
            await contextAsB.SaveChangesAsync();

            //هختبر كأنى tenantA

            await using var verifyContext = new AppDbContext(options, fakeTenantProvider);
            var visibleRequests = await verifyContext.CitizenRequests.ToListAsync();
            //اتأكد إن اللي رجع فعلاً مطابق لتوقعاتنا، وإلا اعتبر الاختبار فاشل

            Assert.Single(visibleRequests); //اتأكد إن الليستة فيها عنصر واحد بس
            //اتأكد كمان إن العنصر ده هو فعلاً بتاع أحمد، مش سارة بالغلط"
            Assert.Equal("Ahmed", visibleRequests[0].CitizenName); 
        }
        public class FakeTenantProviderForTest : ITenantProvider
        {
            private readonly Guid _tenantId;

            public FakeTenantProviderForTest(Guid tenantId)
            {
                _tenantId = tenantId;
            }

            public Guid GetCurrentTenantId() => _tenantId;
        }
    }
}

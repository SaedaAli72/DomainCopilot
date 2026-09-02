using DomainCopilot.Application.Agents;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Infrastructure.Embeddings;
using DomainCopilot.Infrastructure.Llm;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Infrastructure.Repositories;
using DomainCopilot.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace DomainCopilot.Tests.Integration
{
    public class EligibilityAgentTests
    {
        [Fact]
        public async Task AnalyzeAsync_ShouldReturnEligible_ForIncomeUnderLimit()
        {
            var tenantId = Guid.NewGuid();
            var fakeTenantProvider = new TenantIsolationTests.FakeTenantProviderForTest(tenantId);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options, fakeTenantProvider);

            var document = new Document(tenantId, "Policy.pdf", "v1");
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var chunk = new DocumentChunk(document.Id, "الحد الأقصى للدخل الشهري المسموح به للأسرة هو 8000 جنيه مصري.", 1, "§2.1");
            context.DocumentChunks.Add(chunk);
            await context.SaveChangesAsync();

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<DomainCopilot.Api.Program>()
                .Build();

            var httpClient = new HttpClient();
            var embeddingService = new GeminiEmbeddingService(httpClient, configuration);
            var llmClient = new GeminiLlmClient(httpClient, configuration);
            var vectorStore = new SqlVectorStore(context);
            var chunkRepository = new EfDocumentChunkRepository(context);

            var chunkEmbedding = await embeddingService.EmbedAsync(chunk.Content);
            await vectorStore.IndexAsync(chunk.Id, chunkEmbedding, tenantId);

            var agent = new EligibilityIdentifierAgent(embeddingService, vectorStore, chunkRepository, llmClient);

            var result = await agent.AnalyzeAsync("المواطن دخله الشهري 6000 جنيه", tenantId);

            Assert.True(result.IsEligible);
            Assert.False(result.RequiresEscalation);
        }
    }
}


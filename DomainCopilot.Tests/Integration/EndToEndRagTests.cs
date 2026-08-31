

using DomainCopilot.Application.UseCases;
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
    public class EndToEndRagTests
    {
        [Fact]
        public async Task AskQuestionUseCase_ShouldReturnGroundedAnswer_UsingRealGeminiAndRealChunk()
        {
            // 1. Setup: in-memory database + fixed tenant
            var tenantId = Guid.NewGuid();
            var fakeTenantProvider = new TenantIsolationTests.FakeTenantProviderForTest(tenantId);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new AppDbContext(options, fakeTenantProvider);

            // 2. Seed a real document + chunk with known content
            var document = new Document(tenantId, "Housing_Policy_Test.pdf", "v1");
            context.Documents.Add(document);
            await context.SaveChangesAsync();

            var chunk = new DocumentChunk(document.Id, "الحد الأقصى للدخل الشهري المسموح به للأسرة هو 8000 جنيه مصري.", 1, "§2.1");
            context.DocumentChunks.Add(chunk);
            await context.SaveChangesAsync();

            // 3. Build real services, all sharing the same in-memory context
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<DomainCopilot.Api.Program>()
                .Build();

            var httpClient = new HttpClient();
            var embeddingService = new GeminiEmbeddingService(httpClient, configuration);
            var llmClient = new GeminiLlmClient(httpClient, configuration);
            var vectorStore = new SqlVectorStore(context);
            var chunkRepository = new EfDocumentChunkRepository(context);

            // 4. Real embedding for the chunk, then index it in the vector store
            var chunkEmbedding = await embeddingService.EmbedAsync(chunk.Content);
            await vectorStore.IndexAsync(chunk.Id, chunkEmbedding, tenantId);

            // 5. Run the full use case with a related question
            var useCase = new AskQuestionUseCase(embeddingService, vectorStore, llmClient, chunkRepository);
            var answer = await useCase.ExecuteAsync("كام أقصى دخل مسموح للأسرة؟", tenantId);

            // 6. The answer should not be empty, and should reflect the real content (8000)
            Assert.False(string.IsNullOrWhiteSpace(answer));
            Assert.Contains("8000", answer);
        }
    }
}

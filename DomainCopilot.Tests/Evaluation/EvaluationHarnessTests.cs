

using DomainCopilot.Application.UseCases;
using DomainCopilot.Infrastructure.Embeddings;
using DomainCopilot.Infrastructure.Llm;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Infrastructure.Repositories;
using DomainCopilot.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace DomainCopilot.Tests.Evaluation
{
    public class EvaluationHarnessTests
    {
        public record GoldenQuestion(int Id, string Question, string ExpectedKeyword, string Category);

        [Fact]
        public async Task RunEvaluationHarness_AndReportResults()
        {
            // Load the golden set from the JSON file
            var jsonPath = Path.Combine(AppContext.BaseDirectory, "Evaluation", "golden-set.json");
            var json = await File.ReadAllTextAsync(jsonPath);
            var questions = JsonSerializer.Deserialize<List<GoldenQuestion>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;

            // Setup: real tenant with our 4 seeded documents already in SQL Server
            var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var fakeTenantProvider = new TenantIsolationTests.FakeTenantProviderForTest(tenantId);

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=.;Database=DomainCopilotDb;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            await using var context = new AppDbContext(options, fakeTenantProvider);

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<DomainCopilot.Api.Program>()
                .Build();

            var httpClient = new HttpClient();
            var embeddingService = new GeminiEmbeddingService(httpClient, configuration);
            var llmClient = new GeminiLlmClient(httpClient, configuration);
            var vectorStore = new SqlVectorStore(context);
            var chunkRepository = new EfDocumentChunkRepository(context);

            var useCase = new AskQuestionUseCase(embeddingService, vectorStore, llmClient, chunkRepository);

            var results = new List<(GoldenQuestion Q, string Answer, bool Passed)>();

            foreach (var q in questions)
            {
                var answer = await useCase.ExecuteAsync(q.Question, tenantId);
                var passed = answer.Contains(q.ExpectedKeyword, StringComparison.OrdinalIgnoreCase);
                results.Add((q, answer, passed));

                await Task.Delay(2000); // wait 2 seconds between questions to respect free-tier rate limits
            }

            // Report
            var passedCount = results.Count(r => r.Passed);
            var report = string.Join("\n", results.Select(r =>
                $"[{(r.Passed ? "PASS" : "FAIL")}] #{r.Q.Id} ({r.Q.Category}): {r.Q.Question}\n  Expected: {r.Q.ExpectedKeyword}\n  Got: {r.Answer.Substring(0, Math.Min(100, r.Answer.Length))}...\n"));

            var summary = $"\n=== EVALUATION HARNESS RESULTS ===\nPassed: {passedCount}/{questions.Count} ({100.0 * passedCount / questions.Count:F1}%)\n\n{report}";

            Console.WriteLine(summary);
            await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "evaluation-report.txt"), summary);

            // We don't assert 100% — we record actual numbers as required by FR-3
            Assert.True(passedCount > 0, "At least some questions should pass.");
        }
    }
}

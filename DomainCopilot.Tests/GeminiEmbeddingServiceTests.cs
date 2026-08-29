

using DomainCopilot.Infrastructure.Embeddings;
using Microsoft.Extensions.Configuration;


namespace DomainCopilot.Tests
{
    public class GeminiEmbeddingServiceTests
    {
        [Fact]
        public async Task EmbedAsync_ShouldReturnNonEmptyVector_ForRealText()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<DomainCopilot.Api.Program>()
                .Build();

            var httpClient = new HttpClient();
            var service = new GeminiEmbeddingService(httpClient, configuration);

            var result = await service.EmbedAsync("هل أنا مؤهل للدعم السكني؟");

            Assert.NotEmpty(result);
        }
    }
}

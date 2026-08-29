

using DomainCopilot.Infrastructure.Llm;
using Microsoft.Extensions.Configuration;

namespace DomainCopilot.Tests
{
    public class GeminiLlmClientTests
    {
        [Fact]
        public async Task CompleteAsync_ShouldReturnNonEmptyAnswer_ForRealPrompt()
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<DomainCopilot.Api.Program>()
                .Build();

            var httpClient = new HttpClient();
            var client = new GeminiLlmClient(httpClient, configuration);

            var result = await client.CompleteAsync("قول مرحبا في جملة واحدة بسيطة");

            Assert.False(string.IsNullOrWhiteSpace(result));
        }
    }
}

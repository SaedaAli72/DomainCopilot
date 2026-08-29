using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Infrastructure.Llm
{
    public class FakeLlmClient : ILlmClient
    {
        public Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
        {
            return Task.FromResult("This is a fake LLM response for testing purposes.");

        }
    }
}

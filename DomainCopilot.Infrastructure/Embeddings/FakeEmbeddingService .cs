
using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Infrastructure.Embeddings
{
    public class FakeEmbeddingService :IEmbeddingService
    {
        public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new float[] { 0.1f, 0.2f, 0.3f });
        }
    }
}

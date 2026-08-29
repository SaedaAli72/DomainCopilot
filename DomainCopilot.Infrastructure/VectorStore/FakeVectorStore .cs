

using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Infrastructure.VectorStore
{
    public class FakeVectorStore : IVectorStore
    {
        public Task IndexAsync(Guid chunkId, float[] embedding, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<List<Guid>> SearchAsync(float[] queryEmbedding, Guid tenantId, int topK, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
        }
    }
}

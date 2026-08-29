

namespace DomainCopilot.Application.Interfaces
{
    internal interface IVectorStore
    {
        // Stores the embedding of a chunk in the vector database, scoped to a specific tenant
        Task IndexAsync(Guid chunkId, float[] embedding, Guid tenantId, CancellationToken cancellationToken = default);

        // Finds the topK closest chunk IDs to the query embedding, within the same tenant only
        Task<List<Guid>> SearchAsync(float[] queryEmbedding, Guid tenantId, int topK, CancellationToken cancellationToken = default);
    }

}

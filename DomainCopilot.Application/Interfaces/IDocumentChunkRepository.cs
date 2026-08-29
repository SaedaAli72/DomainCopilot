
using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Interfaces
{
    public interface IDocumentChunkRepository
    {
        // Fetches the actual chunk content for a list of chunk IDs
        Task<List<DocumentChunk>> GetByIdsAsync(List<Guid> chunkIds, CancellationToken cancellationToken = default);

    }
}

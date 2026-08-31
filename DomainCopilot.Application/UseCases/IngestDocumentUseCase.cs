

using DomainCopilot.Application.Interfaces;
using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.UseCases
{
    public class IngestDocumentUseCase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDocumentChunkRepository _chunkRepository;

        public IngestDocumentUseCase(
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            IDocumentRepository documentRepository,
            IDocumentChunkRepository chunkRepository)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _documentRepository = documentRepository;
            _chunkRepository = chunkRepository;
        }
        public async Task<Guid> ExecuteAsync(Guid tenantId, string fileName, string rawText, CancellationToken cancellationToken = default)
        {
            var document = new Document(tenantId, fileName, "v1");
            await _documentRepository.AddAsync(document, cancellationToken);

            var textChunks = SplitIntoChunks(rawText, maxLength: 500);

            int pageNumber = 1;
            foreach (var text in textChunks)
            {
                var chunk = new DocumentChunk(document.Id, text, pageNumber, "auto");
                await _chunkRepository.AddAsync(chunk, cancellationToken);

                var embedding = await _embeddingService.EmbedAsync(text, cancellationToken);
                await _vectorStore.IndexAsync(chunk.Id, embedding, tenantId, cancellationToken);

                pageNumber++;
            }

            document.MarkAsIndexed();
            await _documentRepository.UpdateAsync(document, cancellationToken);

            return document.Id;
        }

        private static List<string> SplitIntoChunks(string text, int maxLength)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += maxLength)
            {
                chunks.Add(text.Substring(i, Math.Min(maxLength, text.Length - i)));
            }
            return chunks;
        }

    }
}



using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Application.UseCases
{
    public class AskQuestionUseCase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly ILlmClient _llmClient;
        private readonly IDocumentChunkRepository _chunkRepository;
        public AskQuestionUseCase(IEmbeddingService embeddingService, IVectorStore vectorStore, ILlmClient llmClient, IDocumentChunkRepository documentChunkRepository)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _llmClient = llmClient;
            _chunkRepository = documentChunkRepository;
        }
        public async Task<string> ExecuteAsync(string question, Guid tenantId, CancellationToken cancellationToken = default)
        {
            // Step 1: convert the question into numbers
            var queryEmbedding = await _embeddingService.EmbedAsync(question, cancellationToken);
            // Step 2: find closest chunks within this tenant only
           
            var chunkIds = await _vectorStore.SearchAsync(queryEmbedding, tenantId, topK: 5, cancellationToken);

            // Step 3: fetch actual chunk content
            var chunks = await _chunkRepository.GetByIdsAsync(chunkIds, cancellationToken);


            // Step 4: ask the LLM using retrieved context only
            var context = string.Join("\n", chunks.Select(c => c.Content));
            var prompt = $"Answer the question using ONLY the information in the context below. Be direct and specific, quoting exact numbers or facts when available.\n\nContext:\n{context}\n\nQuestion: {question}";
            var answer = await _llmClient.CompleteAsync(prompt, cancellationToken);

            return answer;

        }
    }
}

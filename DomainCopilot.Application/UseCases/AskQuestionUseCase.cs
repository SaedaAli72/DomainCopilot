

using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Application.UseCases
{
    internal class AskQuestionUseCase
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly ILlmClient _llmClient;
        public AskQuestionUseCase(IEmbeddingService embeddingService, IVectorStore vectorStore, ILlmClient llmClient)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _llmClient = llmClient;
        }
        public async Task<string> ExecuteAsync(string question, Guid tenantId, CancellationToken cancellationToken = default)
        {
            // Step 1: convert the question into numbers
            var queryEmbedding = await _embeddingService.EmbedAsync(question, cancellationToken);
            // Step 2: find closest chunks within this tenant only
            var chunkIds = await _vectorStore.SearchAsync(queryEmbedding, tenantId, topK: 5, cancellationToken);

            // Step 4: ask the LLM using retrieved context only
            var prompt = $"Question: {question}\n(context chunks will be injected here)";
            var answer = await _llmClient.CompleteAsync(prompt, cancellationToken);

            return answer;

        }
    }
}

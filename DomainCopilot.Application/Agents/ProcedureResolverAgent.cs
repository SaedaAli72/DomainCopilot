

using DomainCopilot.Application.Agents.Contracts;
using DomainCopilot.Application.Agents.Interfaces;
using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Application.Agents
{
    public class ProcedureResolverAgent :IProcedureResolverAgent
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly IDocumentChunkRepository _chunkRepository;
        private readonly ILlmClient _llmClient;

        public ProcedureResolverAgent(
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            IDocumentChunkRepository chunkRepository,
            ILlmClient llmClient)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _chunkRepository = chunkRepository;
            _llmClient = llmClient;
        }

        public async Task<ProcedureResult> ResolveAsync(string serviceType, Guid tenantId, CancellationToken cancellationToken = default)
        {
            var queryEmbedding = await _embeddingService.EmbedAsync(serviceType, cancellationToken);
            var chunkIds = await _vectorStore.SearchAsync(queryEmbedding, tenantId, topK: 5, cancellationToken);
            var chunks = await _chunkRepository.GetByIdsAsync(chunkIds, cancellationToken);
            var context = string.Join("\n", chunks.Select(c => c.Content));

            var prompt = $"""
            Based ONLY on the context below, list the required documents, estimated timeline, and fees for this service.
            Respond in EXACTLY this format, nothing else:
            DOCUMENTS: [comma-separated list]
            TIMELINE: [e.g. "14 business days"]
            FEES: [amount or "None"]

            Context:
            {context}

            Service: {serviceType}
            """;

            var rawAnswer = await _llmClient.CompleteAsync(prompt, cancellationToken);

            return ParseResponse(rawAnswer);
        }

        private static ProcedureResult ParseResponse(string rawAnswer)
        {
            var lines = rawAnswer.Split('\n');

            var documentsLine = lines.FirstOrDefault(l => l.StartsWith("DOCUMENTS:"))?.Replace("DOCUMENTS:", "").Trim() ?? "";
            var timelineLine = lines.FirstOrDefault(l => l.StartsWith("TIMELINE:"))?.Replace("TIMELINE:", "").Trim() ?? "Unknown";
            var feesLine = lines.FirstOrDefault(l => l.StartsWith("FEES:"))?.Replace("FEES:", "").Trim();

            var documents = documentsLine
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            return new ProcedureResult(documents, timelineLine, feesLine == "None" ? null : feesLine);
        }
    }
}


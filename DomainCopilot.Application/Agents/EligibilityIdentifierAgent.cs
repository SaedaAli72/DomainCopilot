

using DomainCopilot.Application.Agents.Contracts;
using DomainCopilot.Application.Agents.Interfaces;
using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Application.Agents
{
    public class EligibilityIdentifierAgent :IEligibilityIdentifierAgent
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly IDocumentChunkRepository _chunkRepository;
        private readonly ILlmClient _llmClient;

        public EligibilityIdentifierAgent(
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

        public async Task<EligibilityResult> AnalyzeAsync(string citizenSituation, Guid tenantId, CancellationToken cancellationToken = default)
        {
            // Same RAG steps as AskQuestionUseCase: embed, search, fetch real content
            var queryEmbedding = await _embeddingService.EmbedAsync(citizenSituation, cancellationToken);
            var chunkIds = await _vectorStore.SearchAsync(queryEmbedding, tenantId, topK: 5, cancellationToken);
            var chunks = await _chunkRepository.GetByIdsAsync(chunkIds, cancellationToken);
            var context = string.Join("\n", chunks.Select(c => c.Content));

            // Force the LLM to answer in a strict, parseable format
            var prompt = $"""
                        You are an eligibility checker. Your ONLY job is to determine if the citizen meets the eligibility criteria (e.g. income limits) based on the context below.
                        Do NOT consider missing documents or procedural information — that is handled separately. Only escalate if the eligibility criteria itself is unclear or ambiguous.
                        Respond in EXACTLY this format, nothing else:
                        DECISION: [ELIGIBLE / NOT_ELIGIBLE / ESCALATE]
                        REASON: [one sentence reason based on the context]

                        Context:
                        {context}

                        Citizen situation: {citizenSituation}
                        """;

            var rawAnswer = await _llmClient.CompleteAsync(prompt, cancellationToken);

            Console.WriteLine("=== RAW LLM RESPONSE ===");
            Console.WriteLine(rawAnswer);
            Console.WriteLine("========================");

            return ParseResponse(rawAnswer);
        }

        private static EligibilityResult ParseResponse(string rawAnswer)
        {
            var decisionLine = rawAnswer.Split('\n').FirstOrDefault(l => l.StartsWith("DECISION:"))?.Replace("DECISION:", "").Trim();
            var reasonLine = rawAnswer.Split('\n').FirstOrDefault(l => l.StartsWith("REASON:"))?.Replace("REASON:", "").Trim() ?? "No reason provided.";

            return decisionLine switch
            {
                "ELIGIBLE" => new EligibilityResult(true, reasonLine, false),
                "NOT_ELIGIBLE" => new EligibilityResult(false, reasonLine, false),
                _ => new EligibilityResult(false, reasonLine, true) // ESCALATE or unparseable → escalate
            };
        }
    }
}

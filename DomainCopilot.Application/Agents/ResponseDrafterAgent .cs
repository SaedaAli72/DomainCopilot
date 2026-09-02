

using DomainCopilot.Application.Agents.Contracts;
using DomainCopilot.Application.Agents.Interfaces;
using DomainCopilot.Application.Interfaces;

namespace DomainCopilot.Application.Agents
{
    public class ResponseDrafterAgent :IResponseDrafterAgent
    {
        private readonly ILlmClient _llmClient;

        public ResponseDrafterAgent(ILlmClient llmClient)
        {
            _llmClient = llmClient;
        }

        public async Task<DraftedResponse> DraftAsync(EligibilityResult eligibility, ProcedureResult procedure, CancellationToken cancellationToken = default)
        {
            var documentsText = string.Join(", ", procedure.RequiredDocuments);
            var feesText = procedure.Fees ?? "لا توجد رسوم";

            var prompt = $"""
            Write a polite, official response to a citizen in Arabic, based on the following facts.
            Do not invent any information not listed below.

            Eligibility: {(eligibility.IsEligible ? "Eligible" : "Not eligible")}
            Reason: {eligibility.Reason}
            Required documents: {documentsText}
            Timeline: {procedure.EstimatedTimeline}
            Fees: {feesText}
            """;

            var responseText = await _llmClient.CompleteAsync(prompt, cancellationToken);

            return new DraftedResponse(
                ResponseText: responseText,
                CitedChunkIds: new List<string>(), // will be wired once agents share chunk provenance
                Eligibility: eligibility,
                Procedure: procedure
            );
        }
    }
}

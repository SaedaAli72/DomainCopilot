

namespace DomainCopilot.Application.Agents.Contracts
{
    public record DraftedResponse(
    string ResponseText,
    List<string> CitedChunkIds,
    EligibilityResult Eligibility,
    ProcedureResult Procedure
);
}

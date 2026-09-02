

namespace DomainCopilot.Application.Agents.Contracts
{
   public record ProcedureResult(
    List<string> RequiredDocuments,
    string EstimatedTimeline,
    string? Fees
);
}

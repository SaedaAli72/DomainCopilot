

namespace DomainCopilot.Application.Agents.Contracts
{
    public record EligibilityResult
    (
        bool IsEligible,
        string Reason,
        bool RequiresEscalation
    );
}

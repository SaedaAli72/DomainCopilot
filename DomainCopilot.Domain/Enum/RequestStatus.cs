namespace DomainCopilot.Domain.Enum
{
    public enum RequestStatus
    {
        PendingReview,
        Eligible,
        NotEligible,
        Escalated,
        AwaitingApproval,
        Approved,
        Rejected
    }
}

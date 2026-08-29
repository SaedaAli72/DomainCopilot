namespace DomainCopilot.Domain.Enum
{
    internal enum RequestStatus
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

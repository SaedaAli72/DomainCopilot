
using DomainCopilot.Domain.Enum;

namespace DomainCopilot.Domain.Entities
{
    public class CitizenRequest
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Tenant? Tenant { get; private set; }
        public string CitizenName { get; private set; }
        public string ServiceType { get; private set; }
        public RequestStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public CitizenRequest(Guid tenantId, string citizenName, string serviceType)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            CitizenName = citizenName;
            ServiceType = serviceType;
            Status = RequestStatus.PendingReview;
            CreatedAt = DateTime.UtcNow;
        }
        public void MarkAsEligible()
        {
            Status= RequestStatus.Eligible;
        }
        public void MarkAsNotEligible()
        {
            Status = RequestStatus.NotEligible;
        }

        public void Escalate()
        {
            Status = RequestStatus.Escalated;
        }

        public void SubmitForApproval()
        {
            Status = RequestStatus.AwaitingApproval;
        }
        public void Approve()
        {
            if (Status != RequestStatus.AwaitingApproval)
                throw new InvalidOperationException("Only requests awaiting approval can be approved.");

            Status = RequestStatus.Approved;
        }

        public void Reject()
        {
            if (Status != RequestStatus.AwaitingApproval)
                throw new InvalidOperationException("Only requests awaiting approval can be rejected.");

            Status = RequestStatus.Rejected;
        }
        public string? DraftedResponseText { get; private set; }
        public string? EligibilityReason { get; private set; }
        public string? RequiredDocumentsSummary { get; private set; }

        public void AttachDraftedResponse(string responseText, string eligibilityReason, string requiredDocumentsSummary)
        {
            DraftedResponseText = responseText;
            EligibilityReason = eligibilityReason;
            RequiredDocumentsSummary = requiredDocumentsSummary;
            Status = RequestStatus.AwaitingApproval;
        }

    }
}

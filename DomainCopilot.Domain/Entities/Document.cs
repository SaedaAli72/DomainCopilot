

using DomainCopilot.Domain.Enum;

namespace DomainCopilot.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; private set; }
        public Guid TenantId { get; private set; }
        public Tenant? Tenant { get; private set; }
        public string FileName { get; private set; }
        public string Version { get; private set; }
        public DocumentStatus Status { get; private set; }
        public DateTime UploadedAt { get; private set; }

        public Document(Guid tenantId, string fileName, string version)
        {
            Id = Guid.NewGuid();
            TenantId = tenantId;
            FileName = fileName;
            Version = version;
            Status = DocumentStatus.Uploaded;
            UploadedAt = DateTime.UtcNow;
        }

        public void StartProcessing() => Status = DocumentStatus.Processing;
        public void MarkAsIndexed() => Status = DocumentStatus.Indexed;
        public void MarkAsFailed() => Status = DocumentStatus.Failed;
    }
}

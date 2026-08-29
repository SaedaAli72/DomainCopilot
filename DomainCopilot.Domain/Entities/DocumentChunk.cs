

namespace DomainCopilot.Domain.Entities
{
    internal class DocumentChunk
    {
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public Document? Document { get; private set; }
        public string Content { get; private set; }
        public int PageNumber { get; private set; }
        public string SectionReference { get; private set; }

        public DocumentChunk(Guid documentId, string content, int pageNumber, string sectionReference)
        {
            Id = Guid.NewGuid();
            DocumentId = documentId;
            Content = content;
            PageNumber = pageNumber;
            SectionReference = sectionReference;
        }
    }

}

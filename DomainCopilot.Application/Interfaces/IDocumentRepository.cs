

using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Interfaces
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document, CancellationToken cancellationToken = default);
        Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    }
}

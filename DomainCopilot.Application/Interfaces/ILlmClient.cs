

namespace DomainCopilot.Application.Interfaces
{
    public interface ILlmClient
    {
        Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default);

    }
}

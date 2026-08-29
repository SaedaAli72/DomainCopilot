

namespace DomainCopilot.Application.Interfaces
{
    internal interface ILlmClient
    {
        Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default);

    }
}


using static System.Net.Mime.MediaTypeNames;

namespace DomainCopilot.Application.Interfaces
{
    internal interface IEmbeddingService
    {
        // Converts a piece of text into a vector of numbers representing its meaning
         Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
    }

}

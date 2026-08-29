using DomainCopilot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace DomainCopilot.Infrastructure.Embeddings
{
    public class GeminiEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiEmbeddingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API key not configured.");
        }
        public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent";

            var requestBody = new
            {
                model = "models/gemini-embedding-001",
                content = new
                {
                    parts = new[] { new { text } }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("x-goog-api-key", _apiKey);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var embedding = json.GetProperty("embedding").GetProperty("values")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();

            return embedding;
        }
        //public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        //{
        //    // Google's embedding endpoint, authenticated via API key in the URL
        //    var url = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={_apiKey}";

        //    // Build the request body in the exact shape Google expects
        //    var requestBody = new
        //    {
        //        content = new
        //        {
        //            parts = new[] { new { text } }
        //        }
        //    };

        //    // Send the text to Google and wait for the response
        //    var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

        //    // Stop immediately if the request failed (bad key, no internet, etc.)
        //    response.EnsureSuccessStatusCode();

        //    // Parse the raw response into readable JSON
        //    var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        //    // Extract the numeric embedding values from the nested JSON structure
        //    var embedding = json.GetProperty("embedding").GetProperty("values")
        //        .EnumerateArray()
        //        .Select(x => x.GetSingle())
        //        .ToArray();

        //    return embedding;
        //}
    }
}

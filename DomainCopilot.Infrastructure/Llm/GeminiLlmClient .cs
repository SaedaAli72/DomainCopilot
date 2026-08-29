

using DomainCopilot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace DomainCopilot.Infrastructure.Llm
{
    public class GeminiLlmClient :ILlmClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiLlmClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"]
                ?? throw new InvalidOperationException("Gemini API key not configured.");
        }

        public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
        {
            var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

            var requestBody = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
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
            var answer = json.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return answer ?? string.Empty;
        }
    }
}

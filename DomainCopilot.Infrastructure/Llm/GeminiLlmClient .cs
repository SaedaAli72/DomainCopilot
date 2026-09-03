

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

            var response = await SendWithRetryAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var answer = json.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return answer ?? string.Empty;
        }
        private async Task<HttpResponseMessage> SendWithRetryAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int maxRetries = 5;
            int delayMs = 5000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                var clonedRequest = await CloneRequestAsync(request);
                var response = await _httpClient.SendAsync(clonedRequest, cancellationToken);

                if (response.StatusCode != System.Net.HttpStatusCode.TooManyRequests)
                    return response;

                if (attempt == maxRetries) return response;

                await Task.Delay(delayMs, cancellationToken);
                delayMs *= 2;
            }

            throw new InvalidOperationException("Unreachable.");
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);
            if (original.Content != null)
            {
                var content = await original.Content.ReadAsStringAsync();
                clone.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
            }
            foreach (var header in original.Headers)
                clone.Headers.Add(header.Key, header.Value);
            return clone;
        }
    }
}

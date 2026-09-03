using DomainCopilot.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace DomainCopilot.Infrastructure.Embeddings;

public class GeminiEmbeddingService : IEmbeddingService
{
    // "التليفون" اللي هنكلم بيه جوجل عبر الإنترنت
    private readonly HttpClient _httpClient;

    // "كلمة السر" اللي بتثبت هويتنا عند جوجل
    private readonly string _apiKey;

    public GeminiEmbeddingService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        // نجيب الـ API key من مكان سري (User Secrets)، لو مش موجود نوقف فورًا برسالة واضحة (Fail Fast)
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini API key not configured.");
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        // عنوان جوجل اللي هيستقبل الطلب
        var url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent";

        // "محتوى الجواب" اللي هنبعته لجوجل — النص اللي عايزين نحوله لأرقام
        var requestBody = new
        {
            model = "models/gemini-embedding-001",
            content = new
            {
                parts = new[] { new { text } }
            }
        };

        // نجهز الطلب: العنوان + المحتوى
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(requestBody)
        };

        // نحط كلمة السر في الـ Header (مش جوه المحتوى) — زي ختم على ظرف الجواب
        request.Headers.Add("x-goog-api-key", _apiKey);

        // نبعت الطلب فعليًا ونستنى الرد
        var response = await SendWithRetryAsync(request, cancellationToken);

        // لو الرد فيه مشكلة (401, 404, إلخ)، وقفي فورًا برسالة خطأ واضحة
        response.EnsureSuccessStatusCode();

        // نفتح الظرف ونقرا اللي جواه
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);

        // الأرقام "مدفونة" جوه embedding → values، فبنوصل لهم
        var embedding = json.GetProperty("embedding").GetProperty("values")
            .EnumerateArray()                  // امشي على كل رقم واحد واحد
            .Select(x => x.GetSingle())        // حوّليه لرقم عشري عادي (float)
            .ToArray();                        // اجمعيهم كلهم في مصفوفة نهائية

        return embedding;
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
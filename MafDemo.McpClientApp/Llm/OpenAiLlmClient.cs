using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MafDemo.McpClientApp.Llm;

public sealed class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly LlmOptions _options;

    public OpenAiLlmClient(
        HttpClient http,
        IOptions<LlmOptions> options)
    {
        _http = http;
        _options = options.Value;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<TDecision> DecideAsync<TDecision>(
        string systemPrompt,
        string userInput,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userInput }
            },
            response_format = new
            {
                type = "json_object"
            },
            temperature = 0
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync(
            "https://api.openai.com/v1/chat/completions",
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var resultJson =
            doc.RootElement
               .GetProperty("choices")[0]
               .GetProperty("message")
               .GetProperty("content")
               .GetString();

        if (string.IsNullOrWhiteSpace(resultJson))
            throw new InvalidOperationException("LLM returned empty decision.");

        return JsonSerializer.Deserialize<TDecision>(resultJson!)!;
    }
}

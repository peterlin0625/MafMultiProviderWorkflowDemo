using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Llm;

public sealed class MistralLlmProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MistralLlmProvider> _logger;
    private readonly LlmProviderOptions _options;

    public string Name => "Mistral";

    public MistralLlmProvider(
        HttpClient httpClient,
        IOptions<LlmOptions> options,
        ILogger<MistralLlmProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var all = options.Value;

        if (!all.Providers.TryGetValue("Mistral", out var mistralOptions))
        {
            throw new InvalidOperationException("Llm:Providers:Mistral 未設定。");
        }

        _options = mistralOptions;

        if (!string.IsNullOrEmpty(_options.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl!.TrimEnd('/'));
        }

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
    }

    public async Task<string> CompleteAsync(
        string prompt,
        LlmRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        // 非正式簡化版：呼叫 Mistral /chat/completions API
        // 之後可以再依官方 SDK 或模型更新微調 body 格式。
        var model = requestOptions?.Model ?? _options.Model ?? "mistral-small-latest";

        var payload = new
        {
            model,
            messages = new[]
            {
                requestOptions?.SystemPrompt is { Length: > 0 } sp
                    ? new { role = "system", content = sp }
                    : null,
                new { role = "user", content = prompt }
            }.Where(m => m != null)!,
            temperature = requestOptions?.Temperature ?? 0.7f,
            max_tokens = requestOptions?.MaxTokens ?? 1024
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("呼叫 Mistral 模型 {Model}", model);

        using var response = await _httpClient.PostAsync(
            "v1/chat/completions",
            content,
            cancellationToken);

        //response.EnsureSuccessStatusCode();

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Mistral API 呼叫失敗: {StatusCode} - {Body}",
                response.StatusCode, errorBody);

            throw new InvalidOperationException(
                $"Mistral API 呼叫失敗: {response.StatusCode} - {errorBody}");
        }


        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        // 非嚴謹解析：依照 Mistral chat/completions 大致結構取第一個 message
        // 若未來 Mistral 格式有調整，你再來一起微調。
        var text = root
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return text ?? string.Empty;
    }
}

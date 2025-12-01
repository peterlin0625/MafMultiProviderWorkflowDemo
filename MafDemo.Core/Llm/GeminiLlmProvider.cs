using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Llm;

public sealed class GeminiLlmProvider : ILlmProvider
{
    private readonly ILogger<GeminiLlmProvider> _logger;
    private readonly LlmProviderOptions _options;

    public string Name => "Gemini";

    public GeminiLlmProvider(IOptions<LlmOptions> options, ILogger<GeminiLlmProvider> logger)
    {
        _logger = logger;
        if (!options.Value.Providers.TryGetValue("Gemini", out var opt))
        {
            throw new InvalidOperationException("Llm:Providers:Gemini 未設定。");
        }

        _options = opt;
    }

    public Task<string> CompleteAsync(
        string prompt,
        LlmRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        // 之後要接 Google Gemini REST / SDK，可以在這裡實作。
        throw new NotImplementedException("GeminiLlmProvider 目前尚未實作。");
    }
}

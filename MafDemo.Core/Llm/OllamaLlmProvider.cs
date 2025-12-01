using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Llm;

public sealed class OllamaLlmProvider : ILlmProvider
{
    private readonly ILogger<OllamaLlmProvider> _logger;
    private readonly LlmProviderOptions _options;

    public string Name => "Ollama";

    public OllamaLlmProvider(IOptions<LlmOptions> options, ILogger<OllamaLlmProvider> logger)
    {
        _logger = logger;
        if (!options.Value.Providers.TryGetValue("Ollama", out var opt))
        {
            throw new InvalidOperationException("Llm:Providers:Ollama 未設定。");
        }

        _options = opt;
    }

    public Task<string> CompleteAsync(
        string prompt,
        LlmRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        // 之後你要接 http://your-vm:11434/api/chat，就在這裡做 REST 呼叫。
        throw new NotImplementedException("OllamaLlmProvider 目前尚未實作。");
    }
}

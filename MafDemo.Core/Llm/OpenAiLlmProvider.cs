using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Llm;

public sealed class OpenAiLlmProvider : ILlmProvider
{
    private readonly ILogger<OpenAiLlmProvider> _logger;
    private readonly LlmProviderOptions _options;

    public string Name => "OpenAI";

    public OpenAiLlmProvider(IOptions<LlmOptions> options, ILogger<OpenAiLlmProvider> logger)
    {
        _logger = logger;
        if (!options.Value.Providers.TryGetValue("OpenAI", out var opt))
        {
            throw new InvalidOperationException("Llm:Providers:OpenAI 未設定。");
        }

        _options = opt;
    }

    public Task<string> CompleteAsync(
        string prompt,
        LlmRequestOptions? requestOptions = null,
        CancellationToken cancellationToken = default)
    {
        // 之後你若要用 OpenAI / Azure OpenAI，可以在這裡接官方 SDK 或 REST。
        throw new NotImplementedException("OpenAiLlmProvider 目前尚未實作。");
    }
}

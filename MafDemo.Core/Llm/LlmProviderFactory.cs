using Microsoft.Extensions.Options;

namespace MafDemo.Core.Llm;

public sealed class LlmProviderFactory : ILlmProviderFactory
{
    private readonly IReadOnlyDictionary<string, ILlmProvider> _providers;
    private readonly LlmOptions _options;

    public LlmProviderFactory(
        IEnumerable<ILlmProvider> providers,
        IOptions<LlmOptions> options)
    {
        _providers = providers.ToDictionary(p => p.Name, p => p,
            StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
    }

    public ILlmProvider GetProvider(string name)
    {
        if (!_providers.TryGetValue(name, out var provider))
        {
            throw new InvalidOperationException(
                $"找不到名稱為 '{name}' 的 LLM Provider。已註冊的 Provider 有: {string.Join(", ", _providers.Keys)}");
        }

        return provider;
    }

    public ILlmProvider GetDefaultProvider()
    {
        if (string.IsNullOrWhiteSpace(_options.DefaultProvider))
        {
            throw new InvalidOperationException("LlmOptions.DefaultProvider 未設定。");
        }

        return GetProvider(_options.DefaultProvider);
    }
}

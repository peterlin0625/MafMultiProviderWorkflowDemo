namespace MafDemo.Core.Llm;

public interface ILlmProviderFactory
{
    /// <summary>
    /// 依名稱取得 Provider，例如 "Mistral"
    /// </summary>
    ILlmProvider GetProvider(string name);

    /// <summary>
    /// 取得預設 Provider（由 LlmOptions.DefaultProvider 決定）
    /// </summary>
    ILlmProvider GetDefaultProvider();
}

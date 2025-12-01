using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Llm;

public interface ILlmProvider
{
    /// <summary>
    /// Provider 名稱，例如: "OpenAI", "Mistral", "Ollama", "Gemini"
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 單輪 Chat Completion 介面，之後會由 MAF Adapter 來呼叫
    /// </summary>
    Task<string> CompleteAsync(
        string prompt,
        LlmRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.Core.Llm;

public sealed class LlmOptions
{
    /// <summary>
    /// 預設使用的 Provider 名稱，例如: "Mistral", "OpenAI", "Ollama", "Gemini"
    /// </summary>
    public string DefaultProvider { get; set; } = "Mistral";

    public string DefaultModel { get; set; } = "mistral-small-latest";

    /// <summary>
    /// 各 Provider 的設定
    /// </summary>
    public Dictionary<string, LlmProviderOptions> Providers { get; set; } = new();
}

public sealed class LlmProviderOptions
{
    public string Type { get; set; } = string.Empty;   // "Mistral" / "OpenAI" / ...

    /// <summary>
    /// 例如 API Key、Bearer Token…
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// BaseUrl，例如
    /// - OpenAI: https://api.openai.com/v1
    /// - Mistral: https://api.mistral.ai/v1
    /// - Ollama: http://localhost:11434
    /// - Gemini: https://generativelanguage.googleapis.com/v1
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// 模型名稱，例如:
    /// - gpt-4.1-mini
    /// - mistral-small-latest
    /// - llama3.1
    /// - gemini-1.5-pro
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 額外可選設定，例如 ProjectId / Organization 等
    /// </summary>
    public Dictionary<string, string> Extra { get; set; } = new();
}

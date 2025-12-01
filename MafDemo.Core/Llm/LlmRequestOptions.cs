using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.Core.Llm;

public sealed class LlmRequestOptions
{
    public string? Model { get; set; }

    public float? Temperature { get; set; }

    public float? TopP { get; set; }

    public int? MaxTokens { get; set; }

    /// <summary>
    /// 系統指示，例如 "你是一個 .NET / MAF 專家，回答請用繁體中文"
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// 預留：未來如果要做 Tool Calling / MCP，這裡可以加上工具描述
    /// </summary>
    public Dictionary<string, object>? Tools { get; set; }
}

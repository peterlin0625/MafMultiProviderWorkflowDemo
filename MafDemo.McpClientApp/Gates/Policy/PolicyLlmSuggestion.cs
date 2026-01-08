using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

public sealed class PolicyLlmSuggestion
{
    public List<string> SuggestedTags { get; init; } = new();

    /// <summary>
    /// 0.0 ~ 1.0：語意相似度信心值（不是規則命中率）
    /// </summary>
    public double Confidence { get; init; }

    public string? Notes { get; init; }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Domain.DomainGate;

/// <summary>
/// 一次 Domain Gate 評估的結果
/// </summary>
public sealed class DomainGateResult
{
    public DomainClassification Classification { get; }

    /// <summary>
    /// 原始使用者輸入（for audit / trace）
    /// </summary>
    public string UserInput { get; }

    /// <summary>
    /// 可選的補充說明（for log / UX）
    /// </summary>
    public string? Reason { get; }

    private DomainGateResult(
        DomainClassification classification,
        string userInput,
        string? reason)
    {
        Classification = classification;
        UserInput = userInput;
        Reason = reason;
    }

    public static DomainGateResult InDomain(string userInput)
        => new(DomainClassification.InDomain, userInput, null);

    public static DomainGateResult Adjacent(string userInput, string? reason = null)
        => new(DomainClassification.AdjacentDomain, userInput, reason);

    public static DomainGateResult Out(string userInput, string? reason = null)
        => new(DomainClassification.OutOfDomain, userInput, reason);
}

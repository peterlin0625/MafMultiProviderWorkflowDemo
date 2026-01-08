using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

public enum PolicyDecisionKind
{
    Allowed,
    Blocked,
    RequiresHumanReview
}

public enum PolicyRiskLevel
{
    Low,
    Medium,
    High
}

public sealed class PolicyDecision
{
    public PolicyDecisionKind Decision { get; init; }
    public PolicyReasonCode ReasonCode { get; init; }
    public PolicyRiskLevel RiskLevel { get; init; } = PolicyRiskLevel.Low;

    /// <summary>
    /// 可用於 Audit / UX（避免暴露內規可在 UX 層再做轉換）
    /// </summary>
    public string ReasonMessage { get; init; } = string.Empty;

    /// <summary>
    /// 用於 Elastic 聚合/查詢，例如 ["political","adult","ip"]
    /// </summary>
    public IReadOnlyList<string> PolicyTags { get; init; } = Array.Empty<string>();

    public static PolicyDecision Allow(string message = "Allowed")
        => new()
        {
            Decision = PolicyDecisionKind.Allowed,
            ReasonCode = PolicyReasonCode.ALLOWED,
            RiskLevel = PolicyRiskLevel.Low,
            ReasonMessage = message,
            PolicyTags = Array.Empty<string>()
        };

    public static PolicyDecision Block(PolicyReasonCode code, string message, params string[] tags)
        => new()
        {
            Decision = PolicyDecisionKind.Blocked,
            ReasonCode = code,
            RiskLevel = PolicyRiskLevel.High,
            ReasonMessage = message,
            PolicyTags = tags
        };

    public static PolicyDecision Review(PolicyReasonCode code, string message, params string[] tags)
        => new()
        {
            Decision = PolicyDecisionKind.RequiresHumanReview,
            ReasonCode = code,
            RiskLevel = PolicyRiskLevel.High,
            ReasonMessage = message,
            PolicyTags = tags
        };
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

/// <summary>
/// 正式 Audit 事件模型（C-3 正式版）
/// </summary>
public sealed class AuditEvent
{
    public string CorrelationId { get; init; } = default!;

    public AuditSubjectType SubjectType { get; init; }

    public string? SubjectName { get; init; }

    /// <summary>
    /// DomainClassification | Decision | DecisionFailed | Execution | ExecutionFailed | ...
    /// </summary>
    public string Phase { get; init; } = default!;

    public object? Payload { get; init; }

    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
}

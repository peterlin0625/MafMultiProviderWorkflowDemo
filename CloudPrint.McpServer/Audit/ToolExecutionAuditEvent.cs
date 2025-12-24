using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Audit;

public sealed class ToolExecutionAuditEvent
{
    public string CorrelationId { get; init; } = default!;
    public string ToolCallId { get; init; } = default!;
    public string ToolName { get; init; } = default!;
    public string Phase { get; init; } = default!; // Start | Success | Failed
    public object? Payload { get; init; }
    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
}

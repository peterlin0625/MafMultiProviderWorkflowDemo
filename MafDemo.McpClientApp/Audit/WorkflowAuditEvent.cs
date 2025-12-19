using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

public sealed class WorkflowAuditEvent
{
    public string CorrelationId { get; init; } = default!;
    public string WorkflowName { get; init; } = default!;
    public string Phase { get; init; } = default!; // Decision | Execution | Failed
    public object? Payload { get; init; }
    public DateTimeOffset At { get; init; } = DateTimeOffset.UtcNow;
}


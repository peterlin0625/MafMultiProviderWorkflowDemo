using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Workflows;

public sealed class WorkflowContext
{
    public string CorrelationId { get; }

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    public WorkflowContext(string correlationId)
    {
        CorrelationId = correlationId;
    }

    public static WorkflowContext New()
        => new WorkflowContext($"wf-{Guid.NewGuid():N}");
}

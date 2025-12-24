using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Audit;

public sealed class InMemoryToolExecutionAuditSink
    : IToolExecutionAuditSink
{
    private readonly List<ToolExecutionAuditEvent> _events = new();

    public Task WriteAsync(ToolExecutionAuditEvent evt)
    {
        _events.Add(evt);
        return Task.CompletedTask;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

public sealed class InMemoryWorkflowAuditSink : IWorkflowAuditSink
{
    private readonly List<WorkflowAuditEvent> _events = new();
    public Task WriteAsync(WorkflowAuditEvent evt)
    {
        _events.Add(evt);
        return Task.CompletedTask;
    }
}

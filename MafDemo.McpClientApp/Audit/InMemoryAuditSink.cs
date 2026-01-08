using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

public sealed class InMemoryAuditSink : IAuditSink
{
    private readonly List<AuditEvent> _events = new();

    public Task WriteAsync(AuditEvent evt)
    {
        _events.Add(evt);
        return Task.CompletedTask;
    }

    // 可選：提供讀取（測試 / Debug）
    public IReadOnlyList<AuditEvent> Events => _events;
}

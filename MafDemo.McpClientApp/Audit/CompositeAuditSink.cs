using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

/// <summary>
/// Fan-out Audit Sink（可同時寫入多個 Sink）
/// </summary>
public sealed class CompositeAuditSink : IAuditSink
{
    private readonly IReadOnlyList<IAuditSink> _sinks;

    public CompositeAuditSink(IEnumerable<IAuditSink> sinks)
    {
        _sinks = sinks.ToList();
    }

    public async Task WriteAsync(AuditEvent evt)
    {
        foreach (var sink in _sinks)
        {
            await sink.WriteAsync(evt);
        }
    }
}


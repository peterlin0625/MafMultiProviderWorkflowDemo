using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Audit;

public sealed class SerilogToolExecutionAuditSink
    : IToolExecutionAuditSink
{
    private readonly ILogger _logger;

    public SerilogToolExecutionAuditSink()
    {
        _logger = Log.ForContext("AuditType", "ToolExecution");
    }

    public Task WriteAsync(ToolExecutionAuditEvent evt)
    {
        _logger.Information(
            "ToolExecution {@Event}",
            new
            {
                evt.CorrelationId,
                evt.ToolCallId,
                evt.ToolName,
                evt.Phase,
                evt.Payload,
                evt.At
            });

        return Task.CompletedTask;
    }
}

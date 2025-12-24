using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Observability;

public sealed class ToolCallContext
{
    public string ToolCallId { get; }
    public string CorrelationId { get; }

    public ToolCallContext(
        string toolCallId,
        string correlationId)
    {
        ToolCallId = toolCallId;
        CorrelationId = correlationId;
    }
}

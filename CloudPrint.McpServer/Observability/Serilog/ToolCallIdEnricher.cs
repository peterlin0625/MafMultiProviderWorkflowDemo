using System;
using System.Collections.Generic;
using System.Text;

using Serilog.Core;
using Serilog.Events;
using CloudPrint.McpServer.Observability;

namespace CloudPrint.McpServer.Observability.Serilog;

/// <summary>
/// Automatically enriches logs with ToolCallId from ToolCallContext.
/// </summary>
public sealed class ToolCallIdEnricher : ILogEventEnricher
{
    public const string PropertyName = "ToolCallId";

    public void Enrich(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory)
    {
        var ctx = ToolCallContextAccessorHolder.Current;

        var toolCallId = ctx?.Current?.ToolCallId;

        if (string.IsNullOrWhiteSpace(toolCallId))
            return;

        var property = propertyFactory
            .CreateProperty(PropertyName, toolCallId);

        logEvent.AddPropertyIfAbsent(property);
    }
}

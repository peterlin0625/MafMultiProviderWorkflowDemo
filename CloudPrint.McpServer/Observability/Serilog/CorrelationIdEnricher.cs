using System;
using System.Collections.Generic;
using System.Text;

using Serilog.Core;
using Serilog.Events;
using CloudPrint.McpServer.Observability;

namespace CloudPrint.McpServer.Observability.Serilog;

/// <summary>
/// Automatically enriches logs with CorrelationId from CorrelationContext.
/// </summary>
public sealed class CorrelationIdEnricher : ILogEventEnricher
{
    public const string PropertyName = "CorrelationId";

    public void Enrich(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = CorrelationContext.Current;

        if (string.IsNullOrWhiteSpace(correlationId))
            return;

        var property = propertyFactory
            .CreateProperty(PropertyName, correlationId);

        logEvent.AddPropertyIfAbsent(property);
    }
}

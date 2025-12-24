using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace CloudPrint.McpServer.Observability;

/// <summary>
/// Bridges CorrelationContext into ILogger scope.
/// Ensures CorrelationId appears in all structured logs.
/// </summary>
public sealed class CorrelationLoggingScope : IDisposable
{
    private readonly IDisposable? _scope;

    private CorrelationLoggingScope(
        ILogger logger,
        string correlationId)
    {
        _scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });
    }

    /// <summary>
    /// Begin a logging scope using current CorrelationContext.
    /// If no CorrelationId exists, returns a no-op disposable.
    /// </summary>
    public static IDisposable Begin(
        ILogger logger)
    {
        var correlationId = CorrelationContext.Current;

        if (string.IsNullOrWhiteSpace(correlationId))
            return NoopDisposable.Instance;

        return new CorrelationLoggingScope(logger, correlationId);
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();
        public void Dispose() { }
    }
}

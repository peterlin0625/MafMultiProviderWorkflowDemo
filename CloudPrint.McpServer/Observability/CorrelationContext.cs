using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Observability; 
public static class CorrelationContext
{
    private static readonly AsyncLocal<string?> _current = new();

    public static string? Current => _current.Value;

    internal static IDisposable Begin(string correlationId)
    {
        _current.Value = correlationId;
        return new Reset();
    }

    private sealed class Reset : IDisposable
    {
        public void Dispose() => _current.Value = null;
    }
}

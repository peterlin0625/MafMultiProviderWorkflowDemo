using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Observability;

public sealed class ToolCallContextAccessor
{
    private static readonly AsyncLocal<ToolCallContext?> _current = new();

    public ToolCallContext? Current => _current.Value;

    internal IDisposable Begin(ToolCallContext context)
    {
        _current.Value = context;
        return new Reset();
    }

    private sealed class Reset : IDisposable
    {
        public void Dispose() => _current.Value = null;
    }
}

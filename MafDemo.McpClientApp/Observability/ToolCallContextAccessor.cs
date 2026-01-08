using MafDemo.McpClientApp.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Observability;

public static class ToolCallContextAccessor
{
    private static readonly AsyncLocal<ToolCallContext?> _current = new();

    public static ToolCallContext? Current => _current.Value;

    public static IDisposable Begin(ToolCallContext context)
    {
        _current.Value = context;
        return new Reset();
    }

    private sealed class Reset : IDisposable
    {
        public void Dispose()
        {
            _current.Value = null;
        }
    }
}


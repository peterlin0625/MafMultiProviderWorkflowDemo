using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Adapters;
public interface IToolClient
{
    Task<object> CallAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        string toolCallId,
        CancellationToken ct);
}

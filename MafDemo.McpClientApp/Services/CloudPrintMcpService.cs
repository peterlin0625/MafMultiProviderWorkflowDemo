using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Services;

public sealed class CloudPrintMcpService
{
    private readonly ToolInvoker _toolInvoker;

    public CloudPrintMcpService(ToolInvoker toolInvoker)
    {
        _toolInvoker = toolInvoker;
    }

    public async Task<object> GetServerTimeAsync(
    CancellationToken cancellationToken)
    {
        var toolCallId = $"tool-{Guid.NewGuid():N}";

        var context = new ToolCallContext(
            toolCallId: toolCallId,
            toolName: "getServerTime",
            isSideEffect: false,
            idempotencyExpected: true
        );

        IReadOnlyDictionary<string, object?> arguments =
            new Dictionary<string, object?>();

        return await _toolInvoker.InvokeAsync(
            context,
            arguments,
            cancellationToken);
    }
}

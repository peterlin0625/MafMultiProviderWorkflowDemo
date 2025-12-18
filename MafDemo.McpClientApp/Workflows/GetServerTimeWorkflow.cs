using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Workflows;

public sealed class GetServerTimeWorkflow
    : WorkflowBase<DateTimeOffset>
{
    public GetServerTimeWorkflow(
        ToolInvoker toolInvoker,
        WorkflowContext workflowContext)
        : base(toolInvoker, workflowContext)
    {
    }

    protected override async Task<DateTimeOffset> ExecuteInternalAsync(
        CancellationToken cancellationToken)
    {
        var toolCallId = $"tool-{Guid.NewGuid():N}";

        var context = new ToolCallContext(
            toolCallId: toolCallId,
            correlationId: WorkflowContext.CorrelationId,
            toolName: "getServerTime",
            isSideEffect: false,
            idempotencyExpected: true
        );

        IReadOnlyDictionary<string, object?> arguments =
            new Dictionary<string, object?>();

        var result = await ToolInvoker.InvokeAsync(
            context,
            arguments,
            cancellationToken);

        return DateTimeOffset.Parse(result.ToString()!);
    }
}

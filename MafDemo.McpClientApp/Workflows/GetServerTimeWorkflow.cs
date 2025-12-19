using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Workflows;

public sealed class GetServerTimeWorkflow
    : WorkflowBase<DateTimeOffset>
{
    public override string Name => "GetServerTimeWorkflow";

    public override string Description =>
        "Query current server time. No side effects.";

    public override bool AllowLlmInvocation => true;

    public override bool IsHighRisk => false;

    public override IReadOnlyList<string> RequiredArguments =>
        Array.Empty<string>();

    public GetServerTimeWorkflow(
        ToolInvoker toolInvoker,
        WorkflowContext workflowContext)
        : base(toolInvoker, workflowContext)
    {
    }

    protected override async Task<DateTimeOffset> ExecuteInternalAsync(
       IReadOnlyDictionary<string, object?> arguments,
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

        var result = await ToolInvoker.InvokeAsync(
            context,
            arguments,
            cancellationToken);

        return DateTimeOffset.Parse(result.ToString()!);
    }
}

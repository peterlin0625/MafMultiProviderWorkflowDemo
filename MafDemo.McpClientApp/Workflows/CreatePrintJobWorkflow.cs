using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;


namespace MafDemo.McpClientApp.Workflows;

public sealed class CreatePrintJobWorkflow
    : WorkflowBase<string>
{

    public override string Name => "CreatePrintJobWorkflow";

    public override string Description =>
        "Create a print job. This operation has side effects.";

    public override bool AllowLlmInvocation => true;

    public override bool IsHighRisk => true;

    public override IReadOnlyList<string> RequiredArguments =>
        new[] { "product", "copies" };

    public CreatePrintJobWorkflow(
        ToolInvoker toolInvoker,
        WorkflowContext workflowContext)
        : base(toolInvoker, workflowContext)
    {
    }

    protected override async Task<string> ExecuteInternalAsync(
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken)
    {
        // Step 1️⃣ Query server time
        var timeContext = new ToolCallContext(
            toolCallId: $"tool-{Guid.NewGuid():N}",
            correlationId: WorkflowContext.CorrelationId,
            toolName: "getServerTime",
            isSideEffect: false,
            idempotencyExpected: true
        );

        var timeResult = await ToolInvoker.InvokeAsync(
            timeContext,
            new Dictionary<string, object?>(),
            cancellationToken);

        // Step 2️⃣ Create print job (side-effect)
        var jobContext = new ToolCallContext(
            toolCallId: $"tool-{Guid.NewGuid():N}",
            correlationId: WorkflowContext.CorrelationId,
            toolName: "createPrintJob",
            isSideEffect: true,
            idempotencyExpected: false
        );

        //IReadOnlyDictionary<string, object?> arguments =
        //    new Dictionary<string, object?>
        //    {
        //        ["requestedAt"] = timeResult,
        //        ["productId"] = "A4_POSTER",
        //        ["copies"] = 1
        //    };

        var jobResult = await ToolInvoker.InvokeAsync(
            jobContext,
            arguments,
            cancellationToken);

        return jobResult.ToString()!;
    }
}

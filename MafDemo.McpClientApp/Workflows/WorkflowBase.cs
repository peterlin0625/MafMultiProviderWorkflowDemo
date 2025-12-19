using MafDemo.McpClientApp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Workflows;
  
public abstract class WorkflowBase<TResult> : IWorkflowDefinition
{
    protected ToolInvoker ToolInvoker { get; }
    protected WorkflowContext WorkflowContext { get; }

    protected WorkflowBase(
        ToolInvoker toolInvoker,
        WorkflowContext workflowContext)
    {
        ToolInvoker = toolInvoker;
        WorkflowContext = workflowContext;
    }

    // ===== 能力描述（給 Agent / LLM 用） =====
    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract bool AllowLlmInvocation { get; }

    public abstract bool IsHighRisk { get; }

    public abstract IReadOnlyList<string> RequiredArguments { get; }

    // ===== 執行入口 =====
    public Task<TResult> ExecuteAsync(
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken = default)
        => ExecuteInternalAsync(arguments, cancellationToken);

    protected abstract Task<TResult> ExecuteInternalAsync(
        IReadOnlyDictionary<string, object?> arguments,
        CancellationToken cancellationToken);
}


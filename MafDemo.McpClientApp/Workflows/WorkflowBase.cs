using MafDemo.McpClientApp.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Workflows;

public abstract class WorkflowBase<TResult>
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

    public Task<TResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
        => ExecuteInternalAsync(cancellationToken);

    protected abstract Task<TResult> ExecuteInternalAsync(
        CancellationToken cancellationToken);
}

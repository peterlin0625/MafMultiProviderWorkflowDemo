using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows;

public interface IWorkflowRunner
{
    Task RunAsync<TContext>(
        ISequentialWorkflow<TContext> workflow,
        TContext context,
        CancellationToken cancellationToken = default);
}

public sealed class WorkflowRunner : IWorkflowRunner
{
    private readonly ILogger<WorkflowRunner> _logger;

    public WorkflowRunner(ILogger<WorkflowRunner> logger)
    {
        _logger = logger;
    }

    public async Task RunAsync<TContext>(
        ISequentialWorkflow<TContext> workflow,
        TContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("開始執行 Workflow：{WorkflowName}", workflow.Name);
        await workflow.RunAsync(context, cancellationToken);
        _logger.LogInformation("Workflow {WorkflowName} 執行完成", workflow.Name);
    }
}

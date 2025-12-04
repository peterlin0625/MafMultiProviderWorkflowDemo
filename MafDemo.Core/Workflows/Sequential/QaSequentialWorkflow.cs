using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Sequential;

public sealed class QaSequentialWorkflow : ISequentialWorkflow<QaSequentialContext>
{
    private readonly IReadOnlyList<IWorkflowStep<QaSequentialContext>> _steps;
    private readonly ILogger<QaSequentialWorkflow> _logger;

    public string Name => "QaSequentialWorkflow";

    public QaSequentialWorkflow(
        IEnumerable<IWorkflowStep<QaSequentialContext>> steps,
        ILogger<QaSequentialWorkflow> logger)
    {
        _steps = new List<IWorkflowStep<QaSequentialContext>>(steps);
        _logger = logger;
    }

    public async Task RunAsync(QaSequentialContext context, CancellationToken cancellationToken = default)
    {
        foreach (var step in _steps)
        {
            _logger.LogInformation("開始執行步驟：{StepName}", step.Name);
            await step.ExecuteAsync(context, cancellationToken);
            _logger.LogInformation("步驟 {StepName} 完成", step.Name);
        }
    }
}

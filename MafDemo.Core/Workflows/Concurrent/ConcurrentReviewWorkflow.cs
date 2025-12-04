using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Concurrent;

public sealed class ConcurrentReviewWorkflow : ISequentialWorkflow<ConcurrentReviewContext>
{
    private readonly ILogger<ConcurrentReviewWorkflow> _logger;
    private readonly IReadOnlyList<ExpertExecutionStep> _expertSteps;
    private readonly ReviewMergeStep _mergeStep;

    public string Name => "ConcurrentReviewWorkflow";

    public ConcurrentReviewWorkflow(
        IEnumerable<ExpertExecutionStep> expertSteps,
        ReviewMergeStep mergeStep,
        ILogger<ConcurrentReviewWorkflow> logger)
    {
        _logger = logger;
        _expertSteps = expertSteps.ToList();
        _mergeStep = mergeStep;
    }

    public async Task RunAsync(ConcurrentReviewContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("開始 Concurrent Review Workflow");

        // 1) 並行所有 Expert Step
        var tasks = _expertSteps
            .Select(step => step.ExecuteAsync(context, cancellationToken))
            .ToArray();

        await Task.WhenAll(tasks);

        // 2) 合併結果
        await _mergeStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation("Concurrent Review Workflow 完成");
    }
}

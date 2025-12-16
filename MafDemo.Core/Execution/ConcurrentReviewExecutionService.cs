using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Concurrent;
using MafDemo.Core.Workflows.Sequential;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Execution;

public sealed class ConcurrentReviewExecutionService
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<ConcurrentReviewContext> _workflow;

    public ConcurrentReviewExecutionService(
        IWorkflowRunner runner,
        ISequentialWorkflow<ConcurrentReviewContext> workflow)
    {
        _runner = runner;
        _workflow = workflow;
    }

    public async Task<ConcurrentReviewResult> RunAsync(string question, CancellationToken cancellationToken = default)
    {
        var context = new ConcurrentReviewContext
        {
            OriginalQuestion = question
        };

        await _runner.RunAsync(_workflow, context, cancellationToken);

        return new ConcurrentReviewResult
        {
            Question = question,
            AggregatedAnswer = context.FinalAnswer,
            ExpertAnswers = context.ExpertAnswers // 假設是 Dictionary<string,string>
        };
    }
}

public sealed class ConcurrentReviewResult
{
    public string Question { get; set; } = string.Empty;

    /// <summary>最終合併後的答案</summary>
    public string? AggregatedAnswer { get; set; }

    /// <summary>Key = 專家 Id，Value = 專家回答</summary>
    public IDictionary<string, string>? ExpertAnswers { get; set; }
}

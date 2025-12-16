using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Handoffs;
using MafDemo.Core.Workflows.Sequential;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Execution;

public sealed class HandoffsExecutionService
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<HandoffsContext> _workflow;

    public HandoffsExecutionService(
        IWorkflowRunner runner,
        ISequentialWorkflow<HandoffsContext> workflow)
    {
        _runner = runner;
        _workflow = workflow;
    }

    public async Task<HandoffsResult> RunAsync(string question, CancellationToken cancellationToken = default)
    {
        var context = new HandoffsContext
        {
            OriginalQuestion = question
        };

        await _runner.RunAsync(_workflow, context, cancellationToken);

        return new HandoffsResult
        {
            Question = question,
            RoutingRawResult = context.RoutingRawResult,
            TargetExpertId = context.TargetExpertId,
            ExpertAnswer = context.ExpertAnswer,
            FinalAnswer = context.FinalAnswer
        };
    }
}

public sealed class HandoffsResult
{
    public string Question { get; set; } = string.Empty;

    public string? RoutingRawResult { get; set; }

    /// <summary>Routing 決定要交給哪位專家，例如 CloudPrintFlowExpert / AiImageServiceExpert</summary>
    public string? TargetExpertId { get; set; }

    /// <summary>實際被指派專家的回答</summary>
    public string? ExpertAnswer { get; set; }

    /// <summary>最終回覆（目前多半等於 ExpertAnswer，保留未來再經過 RefineAgent 的空間）</summary>
    public string? FinalAnswer { get; set; }
}

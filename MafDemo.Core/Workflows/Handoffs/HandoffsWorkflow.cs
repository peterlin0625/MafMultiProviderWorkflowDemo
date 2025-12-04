using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Handoffs;

public sealed class HandoffsWorkflow : ISequentialWorkflow<HandoffsContext>
{
    private readonly RoutingStep _routingStep;
    private readonly HandoffStep _handoffStep;
    private readonly HandoffsMergeStep _mergeStep;
    private readonly ILogger<HandoffsWorkflow> _logger;

    public string Name => "HandoffsWorkflow";

    public HandoffsWorkflow(
        RoutingStep routingStep,
        HandoffStep handoffStep,
        HandoffsMergeStep mergeStep,
        ILogger<HandoffsWorkflow> logger)
    {
        _routingStep = routingStep;
        _handoffStep = handoffStep;
        _mergeStep = mergeStep;
        _logger = logger;
    }

    public async Task RunAsync(HandoffsContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("HandoffsWorkflow: 開始執行。");

        await _routingStep.ExecuteAsync(context, cancellationToken);
        await _handoffStep.ExecuteAsync(context, cancellationToken);
        await _mergeStep.ExecuteAsync(context, cancellationToken);

        _logger.LogInformation("HandoffsWorkflow: 執行完成，專家 = {ExpertId}", context.TargetExpertId);
    }
}

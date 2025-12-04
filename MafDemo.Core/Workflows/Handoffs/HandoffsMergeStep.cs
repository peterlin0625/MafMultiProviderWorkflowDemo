using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Handoffs;

public sealed class HandoffsMergeStep : IWorkflowStep<HandoffsContext>
{
    private readonly ILogger<HandoffsMergeStep> _logger;

    public string Name => "HandoffsMergeStep";

    public HandoffsMergeStep(ILogger<HandoffsMergeStep> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(HandoffsContext context, CancellationToken cancellationToken = default)
    {
        // 簡單版：直接使用 ExpertAnswer 當成 FinalAnswer
        context.FinalAnswer = context.ExpertAnswer;
        _logger.LogInformation("HandoffsMergeStep: 直接採用專家 {ExpertId} 的回答作為最終回覆。",
            context.TargetExpertId);

        return Task.CompletedTask;
    }
}

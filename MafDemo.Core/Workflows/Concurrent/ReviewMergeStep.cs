using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Agents;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Concurrent;

public sealed class ReviewMergeStep : IWorkflowStep<ConcurrentReviewContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<ReviewMergeStep> _logger;

    public string Name => "ReviewMerge";

    public ReviewMergeStep(
        IAgentFactory agentFactory,
        ILogger<ReviewMergeStep> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(ConcurrentReviewContext context, CancellationToken cancellationToken = default)
    {
        var agent = _agentFactory.CreateDefaultChatAgent("MergeAgent");

        var sb = new StringBuilder();

        sb.AppendLine("以下是多位專家的意見，請你幫我整理出一份統一、正確、清晰的結論：");
        sb.AppendLine();

        foreach (var (expertId, answer) in context.ExpertAnswers)
        {
            sb.AppendLine($"### {expertId} 意見");
            sb.AppendLine(answer);
            sb.AppendLine();
        }

        var merged = await agent.RunAsync(sb.ToString(), cancellationToken);

        context.FinalAnswer = merged;
    }
}

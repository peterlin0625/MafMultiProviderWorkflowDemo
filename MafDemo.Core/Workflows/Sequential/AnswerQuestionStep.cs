using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Agents;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Sequential;

public sealed class AnswerQuestionStep : IWorkflowStep<QaSequentialContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<AnswerQuestionStep> _logger;

    public string Name => "AnswerQuestion";

    public AnswerQuestionStep(
        IAgentFactory agentFactory,
        ILogger<AnswerQuestionStep> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(QaSequentialContext context, CancellationToken cancellationToken = default)
    {
        var agent = _agentFactory.CreateDefaultChatAgent("AnswerAgent");

        var question = context.RewrittenQuestion ?? context.OriginalQuestion;

        var prompt =
            "請針對下列問題，以專業但易懂的方式回答。" +
            "如果涉及地點或國家，請簡短補充關鍵背景。\n\n" +
            $"問題：{question}";

        _logger.LogInformation("AnswerQuestionStep 開始執行。");

        var answer = await agent.RunAsync(prompt, cancellationToken);

        context.RawAnswer = answer?.Trim();

        _logger.LogInformation("AnswerQuestionStep 完成。");
    }
}

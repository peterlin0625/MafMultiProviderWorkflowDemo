using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Agents;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Sequential;

public sealed class RefineAnswerStep : IWorkflowStep<QaSequentialContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<RefineAnswerStep> _logger;

    public string Name => "RefineAnswer";

    public RefineAnswerStep(
        IAgentFactory agentFactory,
        ILogger<RefineAnswerStep> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(QaSequentialContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.RawAnswer))
        {
            _logger.LogWarning("RefineAnswerStep: RawAnswer 為空，略過此步驟。");
            return;
        }

        var agent = _agentFactory.CreateDefaultChatAgent("RefineAgent");

        var prompt =
            "以下是對使用者問題的初步回答，請幫我：\n" +
            "1. 修正可能的用字不精確之處\n" +
            "2. 讓結構更清楚（可用條列）\n" +
            "3. 保持繁體中文\n\n" +
            "初步回答如下：\n" +
            context.RawAnswer;

        _logger.LogInformation("RefineAnswerStep 開始執行。");

        var refined = await agent.RunAsync(prompt, cancellationToken);

        context.RefinedAnswer = refined?.Trim();

        _logger.LogInformation("RefineAnswerStep 完成。");
    }
}

using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Agents;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Sequential;

public sealed class RewriteQuestionStep : IWorkflowStep<QaSequentialContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<RewriteQuestionStep> _logger;

    public string Name => "RewriteQuestion";

    public RewriteQuestionStep(
        IAgentFactory agentFactory,
        ILogger<RewriteQuestionStep> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(QaSequentialContext context, CancellationToken cancellationToken = default)
    {
        var agent = _agentFactory.CreateDefaultChatAgent("RewriteAgent");

        var prompt =
            "請幫我把下列使用者的問題「重新整理成一句清楚的、" +
            "技術顧問比較容易理解的問題」，不要回答，只要輸出重寫後的問題。\n\n" +
            $"使用者原始問題：{context.OriginalQuestion}";

        _logger.LogInformation("RewriteQuestionStep 開始執行。");

        var rewritten = await agent.RunAsync(prompt, cancellationToken);

        context.RewrittenQuestion = rewritten?.Trim();

        _logger.LogInformation("RewriteQuestionStep 完成。RewrittenQuestion = {Rewritten}",
            context.RewrittenQuestion);
    }
}

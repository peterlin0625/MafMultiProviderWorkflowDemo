using MafDemo.Core.Agents;
using MafDemo.Core.Repository;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Handoffs;

public sealed class HandoffStep : IWorkflowStep<HandoffsContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly IExpertPromptRepository _promptRepo;
    private readonly ILogger<HandoffStep> _logger;

    public string Name => "HandoffStep";

    public HandoffStep(
        IAgentFactory agentFactory,
        IExpertPromptRepository promptRepo,
        ILogger<HandoffStep> logger)
    {
        _agentFactory = agentFactory;
        _promptRepo = promptRepo;
        _logger = logger;
    }

    public async Task ExecuteAsync(HandoffsContext context, CancellationToken cancellationToken = default)
    {
        var expertId = context.TargetExpertId;

        if (string.IsNullOrWhiteSpace(expertId))
        {
            _logger.LogWarning("HandoffStep: TargetExpertId 為空，改用預設 FallbackExpert。");
            expertId = "FallbackExpert";
            context.TargetExpertId = expertId;
        }

        var systemPrompt = _promptRepo.GetPromptFor(expertId);
        var agent = _agentFactory.CreateDefaultChatAgent($"{expertId}Agent");

        var sb = new StringBuilder();

        sb.AppendLine("系統角色說明：");
        sb.AppendLine(systemPrompt);
        sb.AppendLine();

        sb.AppendLine("使用者原始問題：");
        sb.AppendLine(context.OriginalQuestion);
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.ToolResultJson))
        {
            sb.AppendLine("以下是系統已查詢到的 ibon 商品 / 外部資料（JSON）：");
            sb.AppendLine(context.ToolResultJson);
            sb.AppendLine();
            sb.AppendLine("請優先根據這些資料給出具體建議，不要虛構不存在的商品。");
        }

        var prompt = sb.ToString();

        _logger.LogInformation("HandoffsExpertStep: 傳給 {ExpertId} 的 Prompt = {Prompt}",
            context.TargetExpertId, prompt);


        var answer = await agent.RunAsync(prompt, cancellationToken);
        context.ExpertAnswer = answer;
    }
}

using MafDemo.Core.Agents;
using MafDemo.Core.Repository;
using Microsoft.Extensions.Logging;
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

        var prompt =
            $@"系統角色說明：
            {systemPrompt}

            ---
            使用者問題：
            『{context.OriginalQuestion}』
            ";

        _logger.LogInformation("HandoffStep: 將問題交給專家 {ExpertId} 處理。", expertId);

        var answer = await agent.RunAsync(prompt, cancellationToken);
        context.ExpertAnswer = answer;
    }
}

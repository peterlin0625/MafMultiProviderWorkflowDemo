using MafDemo.Core.Agents;
using MafDemo.Core.Repository;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Concurrent;

/// <summary>
/// 代表一個「專家 Agent」，會並行執行此步驟
/// </summary>
public sealed class ExpertExecutionStep : IWorkflowStep<ConcurrentReviewContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<ExpertExecutionStep> _logger;
    private readonly IExpertPromptRepository _promptRepo;

    public string Name { get; }

    private readonly string _expertId;

    public ExpertExecutionStep(
        IAgentFactory agentFactory,
        IExpertPromptRepository promptRepo,
        ILogger<ExpertExecutionStep> logger,
        string expertId)
    {
        _agentFactory = agentFactory;
        _promptRepo = promptRepo;
        _logger = logger;
        _expertId = expertId;
        Name = $"Expert-{expertId}";
    }

    public async Task ExecuteAsync(ConcurrentReviewContext context, CancellationToken cancellationToken = default)
    {
        var agent = _agentFactory.CreateDefaultChatAgent($"{_expertId}Agent");
        var systemPrompt = _promptRepo.GetPromptFor(_expertId);

        var prompt =
            $@"系統角色（由 markdown 指定）：
            {systemPrompt}

            ---
            使用者問題：
            『{context.OriginalQuestion}』
            ";

        _logger.LogInformation("並行執行專家 {ExpertId}", _expertId);

        var answer = await agent.RunAsync(prompt, cancellationToken);

        context.ExpertAnswers[_expertId] = answer;
    }
}
using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Groupchat;
using MafDemo.Core.Workflows.Sequential;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Execution;

public sealed class GroupchatExecutionService
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<GroupchatContext> _workflow;

    public GroupchatExecutionService(
        IWorkflowRunner runner,
        ISequentialWorkflow<GroupchatContext> workflow)
    {
        _runner = runner;
        _workflow = workflow;
    }

    public async Task<GroupchatResult> RunAsync(string question, CancellationToken cancellationToken = default)
    {
        var context = new GroupchatContext
        {
            OriginalQuestion = question
        };

        await _runner.RunAsync(_workflow, context, cancellationToken);

        var messages = context.Messages
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Timestamp)
            .Select(m => new GroupchatMessageDto
            {
                Round = m.Round,
                AgentId = m.AgentId,
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp
            })
            .ToList();

        return new GroupchatResult
        {
            Question = question,
            Messages = messages,
            FinalSummary = context.FinalSummary,
            SupervisorRawDecision = context.SupervisorRawDecision
        };
    }
}

public sealed class GroupchatResult
{
    public string Question { get; set; } = string.Empty;

    public List<GroupchatMessageDto> Messages { get; set; } = new();

    /// <summary>
    /// 最終總結，通常是 Supervisor.summary 的 JSON 字串，
    /// 或系統根據群聊內容產生的文字總結。
    /// </summary>
    public string? FinalSummary { get; set; }

    public string? SupervisorRawDecision { get; set; }
}

public sealed class GroupchatMessageDto
{
    public int Round { get; set; }
    public string AgentId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public System.DateTimeOffset Timestamp { get; set; }
}

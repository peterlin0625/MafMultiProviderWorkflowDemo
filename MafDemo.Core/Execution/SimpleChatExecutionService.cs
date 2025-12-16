using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Agents;

namespace MafDemo.Core.Execution;

public sealed class SimpleChatExecutionService
{
    private readonly IAgentFactory _agentFactory;

    public SimpleChatExecutionService(IAgentFactory agentFactory)
    {
        _agentFactory = agentFactory;
    }

    public async Task<SimpleChatResult> RunAsync(string question, CancellationToken cancellationToken = default)
    {
        var agent = _agentFactory.CreateDefaultChatAgent("ApiSimpleChat");
        var answer = await agent.RunAsync(question, cancellationToken);

        return new SimpleChatResult
        {
            Question = question,
            Answer = answer ?? string.Empty
        };
    }
}

public sealed class SimpleChatResult
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}



namespace MafDemo.McpClientApp.Agents;

public sealed class AgentDecisionResult
{
    public bool Success { get; init; }
    public WorkflowDecision? Decision { get; init; }
    public string? Error { get; init; }

    public static AgentDecisionResult Ok(WorkflowDecision decision)
        => new() { Success = true, Decision = decision };

    public static AgentDecisionResult Fail(string error)
        => new() { Success = false, Error = error };
}

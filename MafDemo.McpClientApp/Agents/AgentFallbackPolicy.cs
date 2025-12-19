using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Agents;

public sealed class AgentFallbackPolicy
{
    public int MaxRetry { get; init; } = 2;

    public bool CanRetry(int attempt) => attempt < MaxRetry;
}

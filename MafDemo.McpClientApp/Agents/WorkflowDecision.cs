using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Agents;

public sealed class WorkflowDecision
{
    public string Workflow { get; init; } = default!;
    public Dictionary<string, object?> Arguments { get; init; } = new();
}

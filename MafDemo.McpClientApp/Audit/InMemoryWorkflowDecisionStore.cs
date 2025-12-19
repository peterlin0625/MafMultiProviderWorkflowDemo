using MafDemo.McpClientApp.Agents;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

public sealed class InMemoryWorkflowDecisionStore
    : IWorkflowDecisionStore
{
    private readonly List<object> _store = new();

    public Task SaveAsync(
        string correlationId,
        string userInput,
        WorkflowDecision decision,
        DateTimeOffset decidedAt)
    {
        _store.Add(new
        {
            correlationId,
            userInput,
            decision,
            decidedAt
        });

        return Task.CompletedTask;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;

using MafDemo.McpClientApp.Agents;

public interface IWorkflowDecisionStore
{
    Task SaveAsync(
        string correlationId,
        string userInput,
        WorkflowDecision decision,
        DateTimeOffset decidedAt);
}

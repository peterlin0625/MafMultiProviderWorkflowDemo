using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Workflows;

public interface IWorkflowDefinition
{
    string Name { get; }

    string Description { get; }

    bool AllowLlmInvocation { get; }

    bool IsHighRisk { get; }

    IReadOnlyList<string> RequiredArguments { get; }
}

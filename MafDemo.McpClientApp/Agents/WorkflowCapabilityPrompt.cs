namespace MafDemo.McpClientApp.Agents;

using MafDemo.McpClientApp.Workflows;
using System.Text;

public static class WorkflowCapabilityPrompt
{
    public static string Build(IEnumerable<IWorkflowDefinition> workflows)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are a decision assistant for the CloudPrint system.");
        sb.AppendLine("You may ONLY choose one of the following workflows.");
        sb.AppendLine();

        foreach (var wf in workflows)
        {
            if (!wf.AllowLlmInvocation)
                continue;

            sb.AppendLine($"Workflow: {wf.Name}");
            sb.AppendLine($"Description: {wf.Description}");
            sb.AppendLine($"High Risk: {wf.IsHighRisk}");

            if (wf.RequiredArguments.Count > 0)
            {
                sb.AppendLine("Required Arguments:");
                foreach (var arg in wf.RequiredArguments)
                    sb.AppendLine($"- {arg}");
            }
            else
            {
                sb.AppendLine("Required Arguments: none");
            }

            sb.AppendLine();
        }

        sb.AppendLine("""
        Respond ONLY in JSON format:
        {
          "workflow": "<WorkflowName>",
          "arguments": { ... }
        }
        """);

        return sb.ToString();
    }
}

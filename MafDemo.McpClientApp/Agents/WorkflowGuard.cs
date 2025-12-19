using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Agents;

using MafDemo.McpClientApp.Workflows;


//Guardrail 要檢查什麼？（最低但實用）
//我們先做「正式系統最低門檻」的三個檢查：
//Workflow 是否存在
//Workflow 是否允許 LLM 觸發
//Workflow 是否屬於高風險（side-effect）而需要額外確認
public sealed class WorkflowGuard
{
    public void Validate(
        IWorkflowDefinition workflow,
        WorkflowDecision decision)
    {
        // 1️⃣ 是否允許由 LLM 觸發
        if (!workflow.AllowLlmInvocation)
        {
            throw new InvalidOperationException(
                $"Workflow '{workflow.Name}' is not allowed to be invoked by LLM.");
        }

        // 2️⃣ 高風險 Workflow 的最小保護
        if (workflow.IsHighRisk && decision.Arguments.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workflow '{workflow.Name}' requires explicit arguments confirmation.");
        }

        // 3️⃣ 基本參數存在性檢查（語意層，不是 schema）
        foreach (var requiredArg in workflow.RequiredArguments)
        {
            if (!decision.Arguments.ContainsKey(requiredArg))
            {
                throw new InvalidOperationException(
                    $"Missing required argument '{requiredArg}' for workflow '{workflow.Name}'.");
            }
        }
    }
}


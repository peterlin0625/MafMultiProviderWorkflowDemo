using MafDemo.McpClientApp.Agents;
using MafDemo.McpClientApp.Audit;
using MafDemo.McpClientApp.Domain.DomainGate;
using MafDemo.McpClientApp.Gates.Policy;
using MafDemo.McpClientApp.HumanInLoop;
using MafDemo.McpClientApp.Llm;
using MafDemo.McpClientApp.Observability;
using MafDemo.McpClientApp.Workflows;
using Microsoft.Extensions.DependencyInjection;
using MafDemo.McpClientApp.Gates.Policy;


namespace MafDemo.McpClientApp.Agents;

public sealed class CloudPrintAgent
{
    private readonly IServiceProvider _sp;
    private readonly ILlmClient _llmClient;
    private readonly IDomainGate _domainGate;

    private readonly IPolicyGate _policyGate;

    // 【新增①】LLM 決策 fallback / retry policy
    private readonly AgentFallbackPolicy _fallbackPolicy;

    // 【新增②】Workflow Decision Audit Store
    private readonly IWorkflowDecisionStore _decisionStore;
    private readonly IAuditSink _auditSink;
    // 【新增③】Workflow Guardrail
    private readonly WorkflowGuard _guard;

    // 【新增④】Human-in-the-loop 使用者確認
    private readonly IUserConfirmationService _confirmation;

    public CloudPrintAgent(
        IServiceProvider serviceProvider,
        ILlmClient llmClient,
        IDomainGate domainGate,
        IPolicyGate policyGate,

        // 【新增①】
        AgentFallbackPolicy fallbackPolicy,

        // 【新增②】
        IWorkflowDecisionStore decisionStore,
        IAuditSink auditSink,
        // 【新增③】
        WorkflowGuard guard,

        // 【新增④】
        IUserConfirmationService confirmation)
    {
        _sp = serviceProvider;
        _llmClient = llmClient;
        _domainGate = domainGate;
        _policyGate = policyGate;
        _fallbackPolicy = fallbackPolicy;
        _decisionStore = decisionStore;
        _auditSink = auditSink;
        _guard = guard;
        _confirmation = confirmation;
    }

    public async Task<object> HandleAsync(
        string userInput,
        CancellationToken cancellationToken)
    {
        // =========================================================
        // Step 0. 建立 CorrelationId（★ NEW：唯一來源）
        // =========================================================
        var correlationId = Guid.NewGuid().ToString("N");

        using var _ = CorrelationContext.Begin(correlationId);
        using var __ = new CorrelationLoggingScope();

        // =========================================================
        // ★ Step 0.5 Domain Gate（NEW）
        // =========================================================
        var domainResult = _domainGate.Evaluate(userInput);

        // ★ Audit: Domain Classification
        await _auditSink.WriteAsync(new AuditEvent
        {
            CorrelationId = correlationId,
            SubjectType = AuditSubjectType.DomainGate,
            SubjectName = null,
            Phase = "DomainClassification",
            Payload = new
            {
                domainResult.Classification,
                domainResult.Reason
            }
        }); 

        switch (domainResult.Classification)
        {
            case DomainClassification.OutOfDomain:
                return new
                {
                    Message = "This request is outside the CloudPrint service scope."
                };

            case DomainClassification.AdjacentDomain:
                return new
                {
                    Message = "Could you clarify your printing-related needs?"
                };

            case DomainClassification.InDomain:
                break; // 放行，進入原本流程
        }

        // =========================================================
        // ★ Step 0.6 Safety / Policy Gate（NEW: Phase 1 Rule-first）
        // =========================================================
        var policyDecision = _policyGate.Evaluate(userInput, domainResult);

        switch (policyDecision.Decision)
        {
            case PolicyDecisionKind.Blocked:
                return new
                {
                    Message = "This request is not allowed by policy."
                };

            case PolicyDecisionKind.RequiresHumanReview:
                return new
                {
                    Message = "This request requires human review before we can proceed."
                };

            case PolicyDecisionKind.Allowed:
            default:
                break; // 放行，進入既有流程
        }

        // === Step 0. 取得所有 Workflow 定義 ===
        var workflows = _sp
            .GetServices<IWorkflowDefinition>()
            .ToList();

        if (workflows.Count == 0)
            throw new InvalidOperationException("No workflows registered.");

        // === Step 1. 自動產生 Prompt（既有設計，未改）===
        var prompt = WorkflowCapabilityPrompt.Build(workflows);

        // === Step 2. LLM 決策（新增：retry + fallback）===
        WorkflowDecision decision;
        var attempt = 0;
        Exception? lastError = null;

        do
        {
            attempt++;

            try
            {
                decision = await _llmClient.DecideAsync<WorkflowDecision>(
                    prompt,
                    userInput,
                    cancellationToken);

                // 【成功就直接跳出 retry loop】
                goto DECISION_OK;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

        } while (_fallbackPolicy.CanRetry(attempt));

        // ★ NEW：Decision 失敗也要 Audit
        await _auditSink.WriteAsync(new AuditEvent
        {
            CorrelationId = correlationId,
            SubjectType = AuditSubjectType.Agent,
            SubjectName = nameof(CloudPrintAgent),
            Phase = "DecisionFailed",
            Payload = lastError?.Message
        });

        throw new InvalidOperationException(
            "LLM decision failed after retries.",
            lastError);

    DECISION_OK:

        // === Step 3. Audit：記錄 LLM 的「原始決策」（新增）===
        await _decisionStore.SaveAsync(
            correlationId: WorkflowContext.New().CorrelationId,
            userInput: userInput,
            decision: decision,
            decidedAt: DateTimeOffset.UtcNow);

        await _auditSink.WriteAsync(new AuditEvent
        {
            CorrelationId = correlationId,
            SubjectType = AuditSubjectType.Workflow,
            SubjectName = decision.Workflow, 
            Phase = "Decision",
            Payload = decision
        });

        // === Step 4. 對應 Workflow（既有，但現在更明確）===
        var workflow = workflows
            .FirstOrDefault(w => w.Name == decision.Workflow)
            ?? throw new InvalidOperationException(
                $"Workflow '{decision.Workflow}' not found.");

        // === Step 5. Guardrail（新增）===
        _guard.Validate(workflow, decision);

        // === Step 6. Human-in-the-loop（新增）===
        if (workflow.IsHighRisk)
        {
            var confirmed = await _confirmation.ConfirmAsync(
                $"You are about to execute high-risk workflow '{workflow.Name}'. Proceed?",
                cancellationToken);

            if (!confirmed)
                throw new OperationCanceledException(
                    "Workflow execution cancelled by user.");
        }


        // =========================================================
        // Step 8. Execute Workflow
        // =========================================================
        try
        {
            var result = await ((dynamic)workflow)
                .ExecuteAsync(decision.Arguments, cancellationToken);

            // ★ NEW：Execution Audit
            await _auditSink.WriteAsync(new AuditEvent
            {
                CorrelationId = correlationId,
                SubjectType = AuditSubjectType.Workflow,
                SubjectName = workflow.Name, 
                Phase = "Execution"
            });

            return result;
        }
        catch (Exception ex)
        {
            // ★ NEW：Execution Failed Audit
            await _auditSink.WriteAsync(new AuditEvent
            {
                CorrelationId = correlationId,
                SubjectType = AuditSubjectType.Workflow,
                SubjectName = workflow.Name, 
                Phase = "ExecutionFailed",
                Payload = ex.Message
            });

            throw;
        }
    }
}

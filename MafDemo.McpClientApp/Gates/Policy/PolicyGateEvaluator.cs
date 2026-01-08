using MafDemo.McpClientApp.Audit;
using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Domain.DomainGate;
using Microsoft.Extensions.Options;

namespace MafDemo.McpClientApp.Gates.Policy;

public sealed class PolicyGateEvaluator : IPolicyGate
{
    private readonly PolicyGateOptions _opt;
    private readonly IPolicyLlmAdvisor _advisor;
    private readonly PolicyDecisionComposer _composer;
    private readonly IAuditSink _auditSink;

    public PolicyGateEvaluator(
        IOptions<PolicyGateOptions> options,
        IPolicyLlmAdvisor advisor,
        PolicyDecisionComposer composer,
        IAuditSink auditSink)
    {
        _opt = options.Value ?? new PolicyGateOptions();
        _advisor = advisor;
        _composer = composer;
        _auditSink = auditSink;
    }

    public PolicyDecision Evaluate(string userInput, DomainGateResult domainResult)
    {
        // Phase 2 advisor 需要 async，因此這裡採同步包裝（不改 IPolicyGate 介面）
        return EvaluateAsync(userInput, domainResult, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    private async Task<PolicyDecision> EvaluateAsync(
        string userInput,
        DomainGateResult domainResult,
        CancellationToken cancellationToken)
    {
        // PolicyGate 前提：只處理 In / Adjacent（Out 已在 Agent return）
        if (string.IsNullOrWhiteSpace(userInput))
            return PolicyDecision.Allow("Empty input treated as allowed (handled by other gates).");

        var normalized = userInput.Trim();

        // =========================
        // Phase 1: Rule-first
        // =========================

        // Block: Political
        if (ContainsAny(normalized, _opt.PoliticalKeywords))
        {
            var d = PolicyDecision.Block(
                PolicyReasonCode.POLITICAL_CONTENT,
                "Political content is not allowed.",
                "political");

            return d;
        }

        // Block: Adult
        if (ContainsAny(normalized, _opt.AdultKeywords))
        {
            var d = PolicyDecision.Block(
                PolicyReasonCode.ADULT_CONTENT,
                "Adult content is not allowed.",
                "adult");

            return d;
        }

        // Block: Hate/Violence
        if (ContainsAny(normalized, _opt.HateOrViolenceKeywords))
        {
            var d = PolicyDecision.Block(
                PolicyReasonCode.HATE_OR_VIOLENCE,
                "Hate or violence content is not allowed.",
                "hate_or_violence");

            return d;
        }

        // Review: IP uncertainty
        if (ContainsAny(normalized, _opt.IpKeywords))
        {
            var d = PolicyDecision.Review(
                PolicyReasonCode.IP_UNCERTAIN,
                "This request may involve IP/licensing concerns and requires human review.",
                "ip");

            return d;
        }

        // Phase 1: No match → Allowed baseline
        var phase1 = PolicyDecision.Allow();

        // =========================
        // Phase 2: Hybrid (LLM advisor, suggest-only)
        // =========================
        if (!_opt.EnablePhase2)
            return phase1;

        var suggestion = await _advisor.SuggestAsync(normalized, cancellationToken);

        if (suggestion is not null)
        {
            // 限制 tags 數量，降低噪音
            if (suggestion.SuggestedTags.Count > _opt.MaxSuggestedTags)
            {
                suggestion = new PolicyLlmSuggestion
                {
                    SuggestedTags = suggestion.SuggestedTags
                        .Take(_opt.MaxSuggestedTags)
                        .ToList(),
                    Confidence = suggestion.Confidence,
                    Notes = suggestion.Notes
                };
            }

            // Audit: PolicyLlmSuggested
            // 注意：CorrelationId 在 Agent 端；這裡取 CorrelationContext.Current
            await _auditSink.WriteAsync(new AuditEvent
            {
                CorrelationId = MafDemo.McpClientApp.Observability.CorrelationContext.Current
                    ?? "N/A",
                SubjectType = AuditSubjectType.PolicyGate,
                SubjectName = null,
                Phase = "PolicyLlmSuggested",
                Payload = new
                {
                    suggestion.SuggestedTags,
                    suggestion.Confidence,
                    suggestion.Notes
                }
            });
        }

        var finalDecision = _composer.Compose(phase1, suggestion, _opt);

        // Audit: PolicyEvaluated（Gate-level）
        await _auditSink.WriteAsync(new AuditEvent
        {
            CorrelationId = MafDemo.McpClientApp.Observability.CorrelationContext.Current
                ?? "N/A",
            SubjectType = AuditSubjectType.PolicyGate,
            SubjectName = null,
            Phase = "PolicyEvaluated",
            Payload = new
            {
                finalDecision.Decision,
                finalDecision.ReasonCode,
                finalDecision.RiskLevel,
                finalDecision.PolicyTags,
                finalDecision.ReasonMessage,
                Phase2Enabled = _opt.EnablePhase2,
                Threshold = _opt.ReviewConfidenceThreshold
            }
        });

        return finalDecision;
    }

    private static bool ContainsAny(string input, string[] keywords)
    {
        if (keywords is null || keywords.Length == 0)
            return false;

        return keywords.Any(k =>
            !string.IsNullOrWhiteSpace(k) &&
            input.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

/// <summary>
/// Phase 2 裁決器：Rule-owned，LLM 只能推向更保守
/// </summary>
public sealed class PolicyDecisionComposer
{
    public PolicyDecision Compose(
        PolicyDecision phase1Decision,
        PolicyLlmSuggestion? suggestion,
        PolicyGateOptions options)
    {
        // 鐵律：Phase 1 若已 Block/Review，Phase 2 不介入
        if (phase1Decision.Decision is PolicyDecisionKind.Blocked
            or PolicyDecisionKind.RequiresHumanReview)
        {
            return phase1Decision;
        }

        // Phase 2 未啟用或沒有建議 → 維持 Phase 1 結果
        if (!options.EnablePhase2)
            return phase1Decision;

        if (suggestion is null)
            return phase1Decision;

        if (suggestion.SuggestedTags.Count == 0)
            return phase1Decision;

        // confidence 門檻：只有高於門檻才升級為 Review
        if (suggestion.Confidence < options.ReviewConfidenceThreshold)
            return phase1Decision;

        // 鐵律：LLM 只能推向更保守 → Review
        return PolicyDecision.Review(
            code: PolicyReasonCode.IP_UNCERTAIN, // 用既有 code（Phase 2 先不擴碼）
            message: "Policy gate requires human review due to semantic risk signals.",
            tags: suggestion.SuggestedTags.ToArray());
    }
}


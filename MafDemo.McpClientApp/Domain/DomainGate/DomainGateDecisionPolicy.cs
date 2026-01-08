using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Domain.DomainGate;

/// <summary>
/// Domain Gate 最終裁決規則（Client-side）
/// Rule-based 永遠優先，LLM 僅能作為建議
/// </summary>
public sealed class DomainGateDecisionPolicy
{
    /// <summary>
    /// 套用最終 Domain 判斷
    /// </summary>
    /// <param name="ruleBasedResult">
    /// Phase 1 的 rule-based 結果（不可為 null）
    /// </param>
    /// <param name="llmSuggestion">
    /// LLM 建議結果（Phase 2，可為 null）
    /// </param>
    /// <returns>
    /// 最終 DomainClassification
    /// </returns>
    public DomainClassification Decide(
        DomainClassification ruleBasedResult,
        DomainClassification? llmSuggestion)
    {
        // === 1️⃣ Rule-based 明確結果，永遠優先 ===
        if (ruleBasedResult == DomainClassification.OutOfDomain)
            return DomainClassification.OutOfDomain;

        if (ruleBasedResult == DomainClassification.InDomain)
            return DomainClassification.InDomain;

        // === 2️⃣ Rule-based 為 Adjacent，才考慮 LLM ===
        if (ruleBasedResult == DomainClassification.AdjacentDomain)
        {
            // 沒有 LLM 建議 → 保守維持 Adjacent
            if (llmSuggestion is null)
                return DomainClassification.AdjacentDomain;

            // LLM 明確建議 InDomain → 升級
            if (llmSuggestion == DomainClassification.InDomain)
                return DomainClassification.InDomain;

            // 其餘情況（Out / Adjacent）→ 維持 Adjacent
            return DomainClassification.AdjacentDomain;
        }

        // 理論上不應該發生，保守處理
        return DomainClassification.AdjacentDomain;
    }
}

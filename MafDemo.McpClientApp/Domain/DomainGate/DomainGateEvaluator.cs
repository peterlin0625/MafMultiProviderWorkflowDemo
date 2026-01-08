using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.McpClientApp.Domain.DomainGate;

/// <summary>
/// Phase 2：Hybrid Domain Gate Evaluator
/// - Rule-based 為第一道門
/// - MAF Single Agent 僅在模糊時提供建議
/// - Client-side Policy 為最終裁決
/// </summary>
public sealed class DomainGateEvaluator : IDomainGate
{
    private static readonly string[] HardOutKeywords =
    {
        "政治", "選舉", "股票", "投資", "加密貨幣",
        "醫療", "生病", "診斷", "法律", "官司",
        "星座", "命理", "算命", "宗教"
    };

    private static readonly string[] PrintKeywords =
    {
        "印", "列印", "打印",
        "照片", "圖片", "檔案",
        "海報", "卡片", "貼紙",
        "明信片", "月曆", "名片",
        "輸出", "紙張"
    };

    private readonly DomainGateLlmAdvisor _llmAdvisor;
    private readonly DomainGateDecisionPolicy _decisionPolicy;

    public DomainGateEvaluator(
        DomainGateLlmAdvisor llmAdvisor,
        DomainGateDecisionPolicy decisionPolicy)
    {
        _llmAdvisor = llmAdvisor;
        _decisionPolicy = decisionPolicy;
    }

    public DomainGateResult Evaluate(string userInput)
    {
        // Phase 2 需要 async，但為了不改介面，這裡採同步包裝
        //這一版為了不破壞 IDomainGate 介面，
        //暫時使用了 GetAwaiter().GetResult()
        return EvaluateAsync(userInput, CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    private async Task<DomainGateResult> EvaluateAsync(
        string userInput,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return DomainGateResult.Out(
                userInput,
                "Empty or whitespace input");
        }

        var normalized = userInput.Trim();

        // === 1️⃣ Rule-based 硬性排除 ===
        if (ContainsAny(normalized, HardOutKeywords))
        {
            return DomainGateResult.Out(
                normalized,
                "Rule-based hard exclusion");
        }

        // === 2️⃣ Rule-based 明確 InDomain ===
        if (ContainsAny(normalized, PrintKeywords))
        {
            return DomainGateResult.InDomain(normalized);
        }

        // === 3️⃣ 模糊情況 → 詢問 MAF 建議 ===
        DomainClassification? llmSuggestion = null;

        try
        {
            llmSuggestion = await _llmAdvisor.SuggestAsync(
                normalized,
                cancellationToken);
        }
        catch
        {
            // 任何 MAF 失敗，一律視為「沒有建議」
            llmSuggestion = null;
        }

        var finalClassification = _decisionPolicy.Decide(
            DomainClassification.AdjacentDomain,
            llmSuggestion);

        return finalClassification switch
        {
            DomainClassification.InDomain =>
                DomainGateResult.InDomain(normalized),

            DomainClassification.OutOfDomain =>
                DomainGateResult.Out(normalized),

            _ =>
                DomainGateResult.Adjacent(
                    normalized,
                    "Hybrid evaluation requires clarification")
        };
    }

    private static bool ContainsAny(string input, string[] keywords)
    {
        return keywords.Any(k =>
            input.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}

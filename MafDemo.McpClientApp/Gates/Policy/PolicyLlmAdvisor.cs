using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

using MafDemo.McpClientApp.Llm;

public sealed class PolicyLlmAdvisor : IPolicyLlmAdvisor
{
    private static readonly HashSet<string> AllowedTags =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "political",
            "adult",
            "hate_or_violence",
            "ip"
        };

    private readonly ILlmClient _llmClient;

    public PolicyLlmAdvisor(ILlmClient llmClient)
    {
        _llmClient = llmClient;
    }

    public async Task<PolicyLlmSuggestion?> SuggestAsync(
        string userInput,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userInput))
            return null;

        PolicyLlmSuggestion raw;

        try
        {
            raw = await _llmClient.DecideAsync<PolicyLlmSuggestion>(
                systemPrompt: PolicyLlmPrompt.BuildSystemPrompt(),
                userInput: userInput,
                cancellationToken: cancellationToken);
        }
        catch
        {
            // Phase 2 失敗：視為沒有建議（不可影響可用性）
            return null;
        }

        // 正規化 + 白名單過濾（防 hallucination）
        var tags = (raw.SuggestedTags ?? new List<string>())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .Where(t => AllowedTags.Contains(t))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var confidence = raw.Confidence;
        if (double.IsNaN(confidence) || double.IsInfinity(confidence))
            confidence = 0.0;

        // clamp 0~1
        if (confidence < 0) confidence = 0;
        if (confidence > 1) confidence = 1;

        return new PolicyLlmSuggestion
        {
            SuggestedTags = tags,
            Confidence = confidence,
            Notes = raw.Notes
        };
    }
}

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.McpClientApp.Domain.DomainGate;

/// <summary>
/// 暫時放在這裡的 MAF Single Agent 介面
/// </summary>
public interface ISingleAgent
{
    Task<string> RunAsync(
        string prompt,
        string userInput,
        CancellationToken cancellationToken);
}

/// <summary>
/// Phase 2：MAF-assisted Domain Gate Advisor
/// - 僅透過 MAF Single Agent
/// - 僅提供「建議」，不做最終裁決
/// </summary>
public sealed class DomainGateLlmAdvisor
{
    private readonly ISingleAgent _singleAgent;

    public DomainGateLlmAdvisor(ISingleAgent singleAgent)
    {
        _singleAgent = singleAgent;
    }

    /// <summary>
    /// 向 MAF Single Agent 請求 Domain 分類建議
    /// </summary>
    public async Task<DomainClassification?> SuggestAsync(
        string userInput,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt();

        // 呼叫 MAF Single Agent（而不是直接 LLM）
        var response = await _singleAgent.RunAsync(
            prompt: prompt,
            userInput: userInput,
            cancellationToken: cancellationToken);

        return ParseResponse(response);
    }

    private static string BuildPrompt()
    {
        return """
        You are a domain classifier for the CloudPrint system.

        Your task is to classify the user's input into exactly ONE of the following values:
        - InDomain
        - AdjacentDomain
        - OutOfDomain

        Rules:
        - Do NOT explain.
        - Do NOT suggest workflows.
        - Do NOT provide any text other than JSON.
        - Respond ONLY in the following JSON format:

        {
          "classification": "InDomain | AdjacentDomain | OutOfDomain"
        }
        """;
    }

    private static DomainClassification? ParseResponse(string response)
    {
        try
        {
            using var doc = JsonDocument.Parse(response);

            if (!doc.RootElement.TryGetProperty("classification", out var prop))
                return null;

            var value = prop.GetString();

            return value switch
            {
                "InDomain" => DomainClassification.InDomain,
                "AdjacentDomain" => DomainClassification.AdjacentDomain,
                "OutOfDomain" => DomainClassification.OutOfDomain,
                _ => null
            };
        }
        catch
        {
            // 任何解析錯誤，一律視為「沒有建議」
            return null;
        }
    }
}

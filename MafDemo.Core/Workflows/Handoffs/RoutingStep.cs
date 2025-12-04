using MafDemo.Core.Agents;
using MafDemo.Core.Repository;
using MafDemo.Core.Workflows.Handoffs;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Handoffs;

public sealed class RoutingStep : IWorkflowStep<HandoffsContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly IExpertPromptRepository _promptRepo;
    private readonly ILogger<RoutingStep> _logger;

    public string Name => "RoutingStep";

    public RoutingStep(
        IAgentFactory agentFactory,
        IExpertPromptRepository promptRepo,
        ILogger<RoutingStep> logger)
    {
        _agentFactory = agentFactory;
        _promptRepo = promptRepo;
        _logger = logger;
    }

    private sealed class RoutingResult
    {
        public string? ExpertId { get; set; }
        public string? Reason { get; set; }
    }

    public async Task ExecuteAsync(HandoffsContext context, CancellationToken cancellationToken = default)
    {
        var agent = _agentFactory.CreateDefaultChatAgent("RoutingAgent");
        var routingPrompt = _promptRepo.GetPromptFor("RoutingExpert");

        var prompt =
$@"系統角色說明：
{routingPrompt}

---
使用者問題：
『{context.OriginalQuestion}』
";

        _logger.LogInformation("RoutingStep: 開始分析要交給哪位專家處理。");

        var raw = await agent.RunAsync(prompt, cancellationToken);
        context.RoutingRawResult = raw;

        _logger.LogInformation("RoutingStep: RoutingExpert 回應原始字串 = {Raw}", raw);

        // 預設先設定為 FallbackExpert，後面若解析成功再覆蓋
        context.TargetExpertId ??= "FallbackExpert";

        try
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                _logger.LogWarning("RoutingStep: RoutingExpert 回覆為空，使用 FallbackExpert。");
                context.TargetExpertId = "FallbackExpert";
                return;
            }

            // 🟡 這一步：先從整段文字中抽出 JSON 區段（會把 ```json ... ``` 外皮去掉）
            var json = ExtractJson(raw);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var routingResult = JsonSerializer.Deserialize<RoutingResult>(json, options);

            if (routingResult?.ExpertId is { Length: > 0 } expertId)
            {
                var normalized = expertId.Trim();

                if (normalized is "CloudPrintFlowExpert" or "IbonDocFormatExpert" or "AiImageServiceExpert" or "FallbackExpert")
                {
                    context.TargetExpertId = normalized;
                    _logger.LogInformation(
                        "RoutingStep: 決定交給專家 {ExpertId}，理由：{Reason}",
                        context.TargetExpertId,
                        routingResult.Reason);
                }
                else
                {
                    _logger.LogWarning(
                        "RoutingStep: expertId={ExpertId} 不在允許清單之中，改用 FallbackExpert。",
                        normalized);
                    context.TargetExpertId = "FallbackExpert";
                }
            }
            else
            {
                _logger.LogWarning("RoutingStep: JSON 解析成功但 expertId 為空，改用 FallbackExpert。");
                context.TargetExpertId = "FallbackExpert";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RoutingStep: 解析 RoutingExpert 回覆時發生例外，改用 FallbackExpert。");
            context.TargetExpertId = "FallbackExpert";
        }
    }

    /// <summary>
    /// 從整段文字中抽出第一個 '{' 到最後一個 '}' 之間的字串，當作 JSON。
    /// 若失敗則回傳原始字串。
    /// 這樣即使模型回傳 ```json ... ``` 也能吃。
    /// </summary>
    private static string ExtractJson(string raw)
    {
        var first = raw.IndexOf('{');
        var last = raw.LastIndexOf('}');

        if (first >= 0 && last > first)
        {
            return raw.Substring(first, last - first + 1);
        }

        return raw;
    }
}

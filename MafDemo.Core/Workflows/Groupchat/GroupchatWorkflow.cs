using MafDemo.Core.Agents;
using MafDemo.Core.Repository;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Groupchat;

public sealed class GroupchatWorkflow : ISequentialWorkflow<GroupchatContext>
{
    private readonly IAgentFactory _agentFactory;
    private readonly IExpertPromptRepository _promptRepo;
    private readonly ILogger<GroupchatWorkflow> _logger;

    // 這裡控制最大輪數：每輪 4 個專家 + 1 次 Supervisor 判斷
    private const int MaxRounds = 3;

    public string Name => "GroupchatWorkflow";

    public GroupchatWorkflow(
        IAgentFactory agentFactory,
        IExpertPromptRepository promptRepo,
        ILogger<GroupchatWorkflow> logger)
    {
        _agentFactory = agentFactory;
        _promptRepo = promptRepo;
        _logger = logger;
    }

    /// <summary>
    /// 給 Supervisor 決策用的解析結果
    /// </summary>
    private sealed class SupervisorDecision
    {
        public string Action { get; set; } = "continue"; // continue / finalize
        public string Reason { get; set; } = "無法解析 Supervisor 決策，預設繼續。";
        public string? SummaryJson { get; set; }
    }

    public async Task RunAsync(GroupchatContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GroupchatWorkflow: 開始執行，使用者問題 = {Question}", context.OriginalQuestion);

        for (int round = 1; round <= MaxRounds; round++)
        {
            _logger.LogInformation("GroupchatWorkflow: 進入第 {Round} 輪討論。", round);

            // 1) 商品顧問（ProductAdvisorAgent）
            var productAdvice = await RunAgentAsync(
                round,
                expertId: "ProductAdvisorAgent",
                agentName: "ProductAdvisorAgent",
                prompt: BuildSharedPrompt(context, round),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(productAdvice))
            {
                context.Messages.Add(new GroupchatMessage
                {
                    Round = round,
                    AgentId = "ProductAdvisorAgent",
                    Role = "assistant",
                    Content = productAdvice,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            // 2) 列印規格顧問（PrintSpecAgent）
            var printSpecAdvice = await RunAgentAsync(
                round,
                expertId: "PrintSpecAgent",
                agentName: "PrintSpecAgent",
                prompt: BuildSharedPrompt(context, round),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(printSpecAdvice))
            {
                context.Messages.Add(new GroupchatMessage
                {
                    Round = round,
                    AgentId = "PrintSpecAgent",
                    Role = "assistant",
                    Content = printSpecAdvice,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            // 3) AI 圖片顧問（AiImageAdvisorAgent）
            var aiImageAdvice = await RunAgentAsync(
                round,
                expertId: "AiImageAdvisorAgent",
                agentName: "AiImageAdvisorAgent",
                prompt: BuildSharedPrompt(context, round),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(aiImageAdvice))
            {
                context.Messages.Add(new GroupchatMessage
                {
                    Round = round,
                    AgentId = "AiImageAdvisorAgent",
                    Role = "assistant",
                    Content = aiImageAdvice,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            // 4) IP 商品顧問（IpProductAgent）
            var ipProductAdvice = await RunAgentAsync(
                round,
                expertId: "IpProductAgent",
                agentName: "IpProductAgent",
                prompt: BuildSharedPrompt(context, round),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(ipProductAdvice))
            {
                context.Messages.Add(new GroupchatMessage
                {
                    Round = round,
                    AgentId = "IpProductAgent",
                    Role = "assistant",
                    Content = ipProductAdvice,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }

            // 5) Supervisor 判斷是否結束 / 繼續
            var supervisorRaw = await RunAgentAsync(
                round,
                expertId: "SupervisorAgent",
                agentName: "SupervisorAgent",
                prompt: BuildSupervisorPrompt(context, round),
                cancellationToken);

            context.SupervisorRawDecision = supervisorRaw;

            _logger.LogInformation("GroupchatWorkflow: Supervisor 第 {Round} 輪決策原始內容 = {Decision}",
                round, supervisorRaw);

            var decision = ParseSupervisorDecision(supervisorRaw);

            if (string.Equals(decision.Action, "finalize", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(decision.SummaryJson))
                {
                    context.FinalSummary = decision.SummaryJson;
                }
                else
                {
                    // ✅ 保底：Supervisor 沒給 summary，就用群聊內容幫他拼一份文字總結
                    context.FinalSummary = BuildAutoSummaryFromMessages(context);
                }

                _logger.LogInformation("GroupchatWorkflow: Supervisor 決定於第 {Round} 輪結束群聊。", round);
                return;
            }

            // 否則 action = continue（或解析失敗）→ 繼續下一輪
        }

        // 如果跑完 MaxRounds Supervisor 還沒有 finalize，就再強制請他總結一次
        _logger.LogInformation("GroupchatWorkflow: 已達最大輪次 {MaxRounds}，請 Supervisor 強制總結。", MaxRounds);

        var finalRaw = await RunAgentAsync(
            MaxRounds,
            expertId: "SupervisorAgent",
            agentName: "SupervisorAgent",
            prompt: BuildSupervisorPrompt(context, MaxRounds, forceFinalize: true),
            cancellationToken);

        context.SupervisorRawDecision = finalRaw;

        var finalDecision = ParseSupervisorDecision(finalRaw);

        context.FinalSummary = !string.IsNullOrWhiteSpace(finalDecision.SummaryJson)
            ? finalDecision.SummaryJson
            : BuildAutoSummaryFromMessages(context);
    }

    private string BuildAutoSummaryFromMessages(GroupchatContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("（Supervisor 未提供 summary，由系統自動彙整群聊重點）");
        sb.AppendLine();
        sb.AppendLine($"使用者問題：{context.OriginalQuestion}");
        sb.AppendLine();
        sb.AppendLine("群聊摘要：");

        var lastMessages = context.Messages
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Timestamp)
            .TakeLast(8); // 取最近 8 則發言

        foreach (var msg in lastMessages)
        {
            sb.AppendLine($"[Round {msg.Round}] {msg.AgentId}: {msg.Content}");
        }

        return sb.ToString();
    }


    /// <summary>
    /// 執行單一專家 Agent：將「系統角色 + 群聊上下文」組成 prompt，送給對應 Provider。
    /// </summary>
    private async Task<string?> RunAgentAsync(
        int round,
        string expertId,
        string agentName,
        string prompt,
        CancellationToken cancellationToken)
    {
        var systemPrompt = _promptRepo.GetPromptFor(expertId);
        var agent = _agentFactory.CreateDefaultChatAgent(agentName);

        var fullPrompt = new StringBuilder();
        fullPrompt.AppendLine("系統角色說明：");
        fullPrompt.AppendLine(systemPrompt);
        fullPrompt.AppendLine();
        fullPrompt.AppendLine("=== 群聊上下文 ===");
        fullPrompt.AppendLine(prompt);

        _logger.LogInformation("GroupchatWorkflow: Round {Round}, 專家 {ExpertId} 開始回應。", round, expertId);

        var answer = await agent.RunAsync(fullPrompt.ToString(), cancellationToken);
        return answer?.Trim();
    }

    /// <summary>
    /// 給四位專家用的共同上下文 prompt：帶使用者原始問題 + 近期群聊摘要。
    /// </summary>
    private string BuildSharedPrompt(GroupchatContext context, int round)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"使用者原始問題：{context.OriginalQuestion}");
        sb.AppendLine();
        sb.AppendLine("目前群聊討論紀錄（摘要）：");

        var recent = context.Messages
            .Where(m => m.Round <= round)
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Timestamp)
            .TakeLast(12); // 只帶最近幾則，避免 prompt 過長

        foreach (var msg in recent)
        {
            sb.AppendLine($"[Round {msg.Round}] {msg.AgentId}: {msg.Content}");
        }

        sb.AppendLine();
        sb.AppendLine("請根據上述內容，扮演你在角色說明中的專家，提出你的回應。");
        sb.AppendLine("你不需要重複其他人的內容，可以補充、修正或深化。");

        return sb.ToString();
    }

    /// <summary>
    /// 給 Supervisor 用的上下文：完整群聊摘要 + 決策指示。
    /// </summary>
    private string BuildSupervisorPrompt(GroupchatContext context, int round, bool forceFinalize = false)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"使用者原始問題：{context.OriginalQuestion}");
        sb.AppendLine();
        sb.AppendLine("目前群聊討論紀錄：");

        var ordered = context.Messages
            .Where(m => m.Round <= round)
            .OrderBy(m => m.Round)
            .ThenBy(m => m.Timestamp);

        foreach (var msg in ordered)
        {
            sb.AppendLine($"[Round {msg.Round}] {msg.AgentId}: {msg.Content}");
        }

        sb.AppendLine();
        if (forceFinalize)
        {
            sb.AppendLine("請務必輸出 action = \"finalize\"，並在 summary 中給出最終完整方案（JSON 物件）。");
        }
        else
        {
            sb.AppendLine("請依據目前討論情況，決定是 action = \"continue\" 還是 \"finalize\"。");
        }

        sb.AppendLine("請嚴格依照系統角色說明中的 JSON 格式輸出，不要加入 ```json 這類程式碼區塊。");

        return sb.ToString();
    }

    /// <summary>
    /// 解析 Supervisor 回傳的 JSON，抓出 action / reason / summary（summary 以 JSON 字串形式保存）。
    /// </summary>
    private SupervisorDecision ParseSupervisorDecision(string? raw)
    {
        var decision = new SupervisorDecision();

        if (string.IsNullOrWhiteSpace(raw))
        {
            return decision;
        }

        try
        {
            var jsonPart = ExtractJson(raw);

            using var doc = JsonDocument.Parse(jsonPart);
            var root = doc.RootElement;

            if (root.TryGetProperty("action", out var actionProp) &&
                actionProp.ValueKind == JsonValueKind.String)
            {
                decision.Action = actionProp.GetString() ?? "continue";
            }

            if (root.TryGetProperty("reason", out var reasonProp) &&
                reasonProp.ValueKind == JsonValueKind.String)
            {
                decision.Reason = reasonProp.GetString() ?? decision.Reason;
            }

            if (root.TryGetProperty("summary", out var summaryProp) &&
                summaryProp.ValueKind != JsonValueKind.Undefined &&
                summaryProp.ValueKind != JsonValueKind.Null)
            {
                // 有 summary 就抓原始 JSON
                decision.SummaryJson = summaryProp.GetRawText();
            }
            else
            {
                // ✅ 沒有 summary，就把整個 jsonPart 當成 summary，至少不是空的
                decision.SummaryJson = jsonPart;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GroupchatWorkflow: 解析 Supervisor 決策失敗，使用預設決策。");

            // ✅ 解析整段 raw 的 JSON 失敗時，至少把 raw 存進 SummaryJson
            decision.SummaryJson = raw;
        }

        return decision;
    }

    /// <summary>
    /// 從整段文字中抽出第一個 '{' 到最後一個 '}' 的 substring 當成 JSON。
    /// 可處理模型輸出中夾雜的說明文字或 ```json 區塊。
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

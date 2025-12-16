namespace MafDemo.Core.Workflows.Handoffs;

public sealed class HandoffsContext
{
    /// <summary>
    /// 使用者原始問題
    /// </summary>
    public string OriginalQuestion { get; set; } = string.Empty;

    /// <summary>
    /// RoutingExpert 的原始判斷文字（完整回答）
    /// </summary>
    public string? RoutingRawResult { get; set; }

    /// <summary>
    /// RoutingExpert 決定要交給哪位專家，例如: "IbonDocFormatExpert"
    /// </summary>
    public string? TargetExpertId { get; set; }

    // 🆕 MCP Tool 的原始 JSON 結果
    public string? ToolResultJson { get; set; }


    /// <summary>
    /// 被指派專家的回答
    /// </summary>
    public string? ExpertAnswer { get; set; }

    /// <summary>
    /// 最終輸出（目前先等於 ExpertAnswer，保留擴充空間）
    /// </summary>
    public string? FinalAnswer { get; set; }
}

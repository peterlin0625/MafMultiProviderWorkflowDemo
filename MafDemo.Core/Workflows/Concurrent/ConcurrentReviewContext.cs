namespace MafDemo.Core.Workflows.Concurrent;

public sealed class ConcurrentReviewContext
{
    public string OriginalQuestion { get; set; } = string.Empty;

    // 專家 ID → 回覆內容
    public Dictionary<string, string> ExpertAnswers { get; set; } = new();

    // 最終合併後的答案
    public string? FinalAnswer { get; set; }
}

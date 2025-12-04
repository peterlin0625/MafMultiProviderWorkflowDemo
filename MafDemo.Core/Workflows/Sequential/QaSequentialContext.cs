namespace MafDemo.Core.Workflows.Sequential;

/// <summary>
/// 問答型 Sequential Workflow 的上下文資料
/// </summary>
public sealed class QaSequentialContext
{
    /// <summary>
    /// 使用者原始問題
    /// </summary>
    public string OriginalQuestion { get; set; } = string.Empty;

    /// <summary>
    /// 由 Rewrite 步驟產生的「重新整理後的問題」
    /// </summary>
    public string? RewrittenQuestion { get; set; }

    /// <summary>
    /// Answer 步驟產生的初版答案
    /// </summary>
    public string? RawAnswer { get; set; }

    /// <summary>
    /// Refine 步驟產生的「潤飾後答案」
    /// </summary>
    public string? RefinedAnswer { get; set; }
}

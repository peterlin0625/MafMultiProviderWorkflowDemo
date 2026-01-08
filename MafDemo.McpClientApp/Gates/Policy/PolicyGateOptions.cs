using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

public sealed class PolicyGateOptions
{
    public const string SectionName = "PolicyGate";

    // Phase 3 ready:
    // These rules may later be sourced from
    // external policy service / policy-as-code engine.



    // ===== Phase 2 (Hybrid) =====
    public bool EnablePhase2 { get; init; } = false;

    /// <summary>
    /// Phase 2 confidence >= 此值，才會把 Allowed 升級成 RequiresHumanReview
    /// </summary>
    public double ReviewConfidenceThreshold { get; init; } = 0.6;

    /// <summary>
    /// 最多取幾個 tags（避免噪音）
    /// </summary>
    public int MaxSuggestedTags { get; init; } = 2;

    /// <summary>政治宣傳/動員等</summary>
    public string[] PoliticalKeywords { get; init; } =
    {
        "政治", "選舉", "競選", "政黨", "候選人", "拉票", "造勢", "投票"
    };

    /// <summary>成人/色情</summary>
    public string[] AdultKeywords { get; init; } =
    {
        "色情", "AV片", "裸照", "情色", "成人內容"
    };

    /// <summary>仇恨/暴力（最小集合，後續可擴）</summary>
    public string[] HateOrViolenceKeywords { get; init; } =
    {
        "仇恨", "歧視", "屠殺", "恐怖攻擊", "爆炸", "殺人"
    };

    /// <summary>IP/授權疑慮（先以關鍵字示意，後續可接權利人清單）</summary>
    public string[] IpKeywords { get; init; } =
    {
        "迪士尼", "漫威", "寶可夢", "皮卡丘", "哆啦A夢", "龍貓"
    };
}

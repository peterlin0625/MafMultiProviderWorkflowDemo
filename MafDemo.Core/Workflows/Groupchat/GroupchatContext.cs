using System.Collections.Generic;

namespace MafDemo.Core.Workflows.Groupchat;

public sealed class GroupchatContext
{
    public string OriginalQuestion { get; set; } = string.Empty;

    public List<GroupchatMessage> Messages { get; set; } = new();

    public string? FinalSummary { get; set; }

    public string? SupervisorRawDecision { get; set; }
}

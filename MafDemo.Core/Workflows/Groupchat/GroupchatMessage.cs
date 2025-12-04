using System;

namespace MafDemo.Core.Workflows.Groupchat;

public sealed class GroupchatMessage
{
    public int Round { get; set; }

    public string AgentId { get; set; } = string.Empty;

    public string Role { get; set; } = "assistant"; // for visualization: user/assistant/system

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

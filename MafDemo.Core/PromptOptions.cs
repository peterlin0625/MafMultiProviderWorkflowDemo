namespace MafDemo.Core;

public sealed class PromptOptions
{
    public string DefaultSystemPrompt { get; set; } = string.Empty;

    // 所有 Expert 的 Markdown 放置根目錄
    public string ExpertsPromptDirectory { get; set; } = "Prompts/Experts";
}


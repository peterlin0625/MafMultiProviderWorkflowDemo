namespace MafDemo.McpClientApp.Llm;

public sealed class LlmOptions
{
    public string Provider { get; init; } = "OpenAI";
    public string Model { get; init; } = "gpt-4.1-mini";
    public string ApiKey { get; init; } = default!;
}

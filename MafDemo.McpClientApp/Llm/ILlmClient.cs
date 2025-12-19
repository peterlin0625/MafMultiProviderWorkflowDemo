namespace MafDemo.McpClientApp.Llm;

public interface ILlmClient
{
    Task<TDecision> DecideAsync<TDecision>(
        string systemPrompt,
        string userInput,
        CancellationToken cancellationToken);
}

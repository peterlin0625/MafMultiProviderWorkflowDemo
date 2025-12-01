using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Llm;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Agents;

public sealed class MafLlmChatAgent : IChatAgent
{
    private readonly ILlmProvider _provider;
    private readonly ILogger<MafLlmChatAgent> _logger;

    public string Name { get; }

    private readonly string _defaultSystemPrompt =
        "你是一個熟悉 .NET 10、MAF (Microsoft Agent Framework)、MCP、" +
        "以及多雲 LLM Provider 的技術顧問，回答請用繁體中文，" +
        "保持專業但語氣友善、簡潔。";

    public MafLlmChatAgent(
        ILlmProvider provider,
        ILogger<MafLlmChatAgent> logger,
        string? name = null)
    {
        _provider = provider;
        _logger = logger;
        Name = name ?? $"MafAgent-{provider.Name}";
    }

    public Task<string> RunAsync(
        string userInput,
        CancellationToken cancellationToken = default)
    {
        // 單輪的情況，就當作只有一則 User 訊息
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, userInput)
        };

        return RunWithHistoryAsync(messages, cancellationToken);
    }

    public async Task<string> RunWithHistoryAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        // 把 history 變成一個 prompt（簡化版）
        // 之後你可以改成「一則一則傳 messages」給 Provider
        var sb = new StringBuilder();

        foreach (var msg in messages)
        {
            var prefix = msg.Role switch
            {
                ChatRole.System => "[System]",
                ChatRole.User => "[User]",
                ChatRole.Assistant => "[Assistant]",
                _ => "[?]"
            };

            sb.AppendLine($"{prefix} {msg.Content}");
        }

        var prompt = sb.ToString();

        _logger.LogInformation(
            "Agent {AgentName} 使用 Provider {ProviderName} 處理請求。",
            Name,
            (_provider?.Name) ?? "Unknown");

        var options = new LlmRequestOptions
        {
            SystemPrompt = _defaultSystemPrompt
        };

        var result = await _provider.CompleteAsync(
            prompt,
            options,
            cancellationToken);

        return result;
    }
}

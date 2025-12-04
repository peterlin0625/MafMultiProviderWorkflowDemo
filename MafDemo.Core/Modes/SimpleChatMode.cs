using System;
using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Agents;
using MafDemo.Core.Llm;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Modes;

public sealed class SimpleChatMode : IAppMode
{
    private readonly IAgentFactory _agentFactory;
    private readonly LlmOptions _llmOptions;
    private readonly ILogger<SimpleChatMode> _logger;

    public string Id => "1";
    public string DisplayName => "模式 1：單一 Agent 對話（Multi-Provider, 預設 Mistral）";

    public SimpleChatMode(
        IAgentFactory agentFactory,
        IOptions<LlmOptions> llmOptions,
        ILogger<SimpleChatMode> logger)
    {
        _agentFactory = agentFactory;
        _llmOptions = llmOptions.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== 模式 1：單一 Agent 對話 ===");
        Console.WriteLine("可用 Provider：");
        foreach (var kvp in _llmOptions.Providers)
        {
            Console.WriteLine($" - {kvp.Key}");
        }
        Console.WriteLine($"預設 Provider：{_llmOptions.DefaultProvider}");
        Console.Write("請輸入要使用的 Provider（直接 Enter 使用預設）：");

        var providerInput = Console.ReadLine();
        var providerName = string.IsNullOrWhiteSpace(providerInput)
            ? _llmOptions.DefaultProvider
            : providerInput.Trim();

        IChatAgent agent;
        try
        {
            if (string.Equals(providerName, _llmOptions.DefaultProvider, StringComparison.OrdinalIgnoreCase))
            {
                agent = _agentFactory.CreateDefaultChatAgent();
            }
            else
            {
                agent = _agentFactory.CreateChatAgent(providerName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"建立 Agent 失敗：{ex.Message}");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"使用 Provider：{providerName}，Agent：{agent.Name}");
        Console.WriteLine("提示：輸入 `q` 或空白可離開此模式，回主選單。");


        while (true)
        {
            Console.WriteLine();
            Console.Write("你：");
            var question = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(question) ||
                string.Equals(question.Trim(), "q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("結束對話，返回主選單。");
                break; // 跳出模式，回到 Program.cs 的主選單 loop
            }

            try
            {
                var answer = await agent.RunAsync(question, cancellationToken);
                Console.WriteLine();
                Console.WriteLine("Agent：");
                Console.WriteLine(answer);
            }
            catch (NotImplementedException nie)
            {
                Console.WriteLine($"這個 Provider 尚未實作完整呼叫邏輯：{nie.Message}");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "呼叫 Agent 發生錯誤");
                Console.WriteLine($"呼叫 Agent 時發生錯誤：{ex.Message}");
                break;
            }
        }
    }
}

using MafDemo.Core.Llm;
using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Groupchat;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Modes;

public sealed class GroupchatWorkflowMode : IAppMode
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<GroupchatContext> _workflow;
    private readonly LlmOptions _llmOptions;
    private readonly ILogger<GroupchatWorkflowMode> _logger;

    public string Id => "5";
    public string DisplayName => "模式 5：Groupchat 群聊腦力激盪";

    public GroupchatWorkflowMode(
        IWorkflowRunner runner,
        ISequentialWorkflow<GroupchatContext> workflow,
        IOptions<LlmOptions> llmOptions,
        ILogger<GroupchatWorkflowMode> logger)
    {
        _runner = runner;
        _workflow = workflow;
        _llmOptions = llmOptions.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== 模式 5：Groupchat 群聊腦力激盪 ===");
        Console.WriteLine($"目前 Default Provider: {_llmOptions.DefaultProvider}");
        Console.WriteLine("提示：輸入 `q` 或空白可返回主選單。");
        Console.WriteLine();

        Console.WriteLine("範例問題建議：");
        Console.WriteLine("  我要製作婚禮相關的小物，有什麼推薦嗎？");
        Console.WriteLine();

        Console.Write("請輸入你的問題：");
        var question = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(question) ||
            string.Equals(question.Trim(), "q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("已取消 Groupchat 模式。");
            return;
        }

        var context = new GroupchatContext
        {
            OriginalQuestion = question!
        };

        try
        {
            await _runner.RunAsync(_workflow, context, cancellationToken);

            Console.WriteLine();
            Console.WriteLine("=== 群聊過程（依時間排序） ===");

            foreach (var msg in context.Messages.OrderBy(m => m.Round).ThenBy(m => m.Timestamp))
            {
                Console.WriteLine($"[Round {msg.Round}] {msg.AgentId}:");
                Console.WriteLine(msg.Content);
                Console.WriteLine();
            }

            Console.WriteLine("=== Supervisor 最終總結 ===");
            Console.WriteLine(context.FinalSummary);
            //Console.WriteLine(JsonDocument.Parse(context.FinalSummary));
            Console.WriteLine();

            Console.WriteLine("=== Groupchat Events JSON（8.6 視覺化用） ===");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(context.Messages, options);
            //Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行 Groupchat Workflow 發生錯誤");
            Console.WriteLine($"執行 Groupchat Workflow 時發生錯誤：{ex.Message}");
        }
    }
}

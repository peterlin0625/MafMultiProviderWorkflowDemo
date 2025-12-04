using MafDemo.Core.Llm;
using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Handoffs;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Modes;

public sealed class HandoffsWorkflowMode : IAppMode
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<HandoffsContext> _workflow;
    private readonly LlmOptions _llmOptions;
    private readonly ILogger<HandoffsWorkflowMode> _logger;

    public string Id => "4";
    public string DisplayName => "模式 4：Handoffs 專家交接 Workflow";

    public HandoffsWorkflowMode(
        IWorkflowRunner runner,
        ISequentialWorkflow<HandoffsContext> workflow,
        IOptions<LlmOptions> llmOptions,
        ILogger<HandoffsWorkflowMode> logger)
    {
        _runner = runner;
        _workflow = workflow;
        _llmOptions = llmOptions.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== 模式 4：Handoffs 專家交接 Workflow ===");
        Console.WriteLine($"目前 Default Provider: {_llmOptions.DefaultProvider}");
        Console.WriteLine("流程：RoutingExpert -> 指派專家 -> 專家回答");
        Console.WriteLine();
        Console.WriteLine("提示：輸入 `q` 或空白可離開此模式。");
        Console.WriteLine();

        Console.Write("請輸入你的問題：");
        var question = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(question) ||
            string.Equals(question.Trim(), "q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("已取消 Handoffs 模式，返回主選單。");
            return;
        }

        var context = new HandoffsContext
        {
            OriginalQuestion = question!
        };

        try
        {
            await _runner.RunAsync(_workflow, context, cancellationToken);

            Console.WriteLine();
            Console.WriteLine("=== Routing 專家判斷 ===");
            Console.WriteLine(context.RoutingRawResult);
            Console.WriteLine();
            Console.WriteLine($"→ 決定交給專家：{context.TargetExpertId}");
            Console.WriteLine();
            Console.WriteLine("=== 專家回答 ===");
            Console.WriteLine(context.FinalAnswer);
            Console.WriteLine("=================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行 Handoffs Workflow 發生錯誤");
            Console.WriteLine($"執行 Handoffs Workflow 時發生錯誤：{ex.Message}");
        }
    }
}

using MafDemo.Core.Modes;
using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Concurrent;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Modes;

public sealed class ConcurrentReviewWorkflowMode : IAppMode
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<ConcurrentReviewContext> _workflow;
    private readonly ILogger<ConcurrentReviewWorkflowMode> _logger;

    public string Id => "3";
    public string DisplayName => "模式 3：Concurrent 多專家並行審查 Workflow";

    public ConcurrentReviewWorkflowMode(
        IWorkflowRunner runner,
        ISequentialWorkflow<ConcurrentReviewContext> workflow,
        ILogger<ConcurrentReviewWorkflowMode> logger)
    {
        _runner = runner;
        _workflow = workflow;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== 模式 3：Concurrent 多專家並行審查 ===");
        Console.WriteLine();
        Console.WriteLine("提示：輸入 `q` 或空白可返回主選單。");
        Console.WriteLine();

        Console.Write("請輸入你的問題：");
        var question = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(question) ||
            string.Equals(question.Trim(), "q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("已取消並行審查模式。");
            return;
        }

        var context = new ConcurrentReviewContext
        {
            OriginalQuestion = question!
        };

        try
        {
            await _runner.RunAsync(_workflow, context);

            Console.WriteLine();
            Console.WriteLine("=== 多專家審查結果 ===");

            foreach (var pair in context.ExpertAnswers)
            {
                Console.WriteLine($"--- {pair.Key} ---");
                Console.WriteLine(pair.Value);
                Console.WriteLine();
            }

            Console.WriteLine("=== 合併後答案 ===");
            Console.WriteLine(context.FinalAnswer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行 Concurrent Workflow 發生錯誤");
            Console.WriteLine($"執行 Workflow 錯誤：{ex.Message}");
        }
    }
}

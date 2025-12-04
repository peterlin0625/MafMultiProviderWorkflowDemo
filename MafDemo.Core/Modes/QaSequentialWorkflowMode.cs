using System;
using System.Threading;
using System.Threading.Tasks;
using MafDemo.Core.Llm;
using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Modes;

public sealed class QaSequentialWorkflowMode : IAppMode
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<QaSequentialContext> _workflow;
    private readonly LlmOptions _llmOptions;
    private readonly ILogger<QaSequentialWorkflowMode> _logger;

    public string Id => "2";
    public string DisplayName => "模式 2：Sequential 問答 Workflow（Rewrite → Answer → Refine）";

    public QaSequentialWorkflowMode(
        IWorkflowRunner runner,
        ISequentialWorkflow<QaSequentialContext> workflow,
        IOptions<LlmOptions> llmOptions,
        ILogger<QaSequentialWorkflowMode> logger)
    {
        _runner = runner;
        _workflow = workflow;
        _llmOptions = llmOptions.Value;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== 模式 2：Sequential 問答 Workflow 測試 ===");
        Console.WriteLine($"目前 Default Provider: {_llmOptions.DefaultProvider}");
        Console.WriteLine("此 Workflow 會依序執行：");
        Console.WriteLine("  1) RewriteQuestionStep");
        Console.WriteLine("  2) AnswerQuestionStep");
        Console.WriteLine("  3) RefineAnswerStep");
        Console.WriteLine();
        Console.WriteLine("提示：輸入 `q` 或空白可取消本次 Workflow，回主選單。");
        Console.WriteLine();


        Console.Write("請輸入你的問題：");
        var question = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(question) ||
            string.Equals(question.Trim(), "q", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("已取消本次 Workflow，返回主選單。");
            return;
        }

        var context = new QaSequentialContext
        {
            OriginalQuestion = question!
        };

        try
        {
            await _runner.RunAsync(_workflow, context, cancellationToken);

            Console.WriteLine();
            Console.WriteLine("=== Workflow 執行結果 ===");
            Console.WriteLine($"[1] 原始問題：{context.OriginalQuestion}");
            Console.WriteLine();
            Console.WriteLine("[2] Rewrite 後問題：");
            Console.WriteLine(context.RewrittenQuestion);
            Console.WriteLine();
            Console.WriteLine("[3] 初步回答：");
            Console.WriteLine(context.RawAnswer);
            Console.WriteLine();
            Console.WriteLine("[4] 潤飾後回答：");
            Console.WriteLine(context.RefinedAnswer);
            Console.WriteLine("=========================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "執行 Sequential Workflow 發生錯誤");
            Console.WriteLine($"執行 Workflow 時發生錯誤：{ex.Message}");
        }
    }
}

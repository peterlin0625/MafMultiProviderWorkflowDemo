using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Sequential;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Execution;

public sealed class QaSequentialExecutionService
{
    private readonly IWorkflowRunner _runner;
    private readonly ISequentialWorkflow<QaSequentialContext> _workflow;

    public QaSequentialExecutionService(
        IWorkflowRunner runner,
        ISequentialWorkflow<QaSequentialContext> workflow)
    {
        _runner = runner;
        _workflow = workflow;
    }

    public async Task<QaSequentialResult> RunAsync(string question, CancellationToken cancellationToken = default)
    {
        var context = new QaSequentialContext
        {
            OriginalQuestion = question
        };

        await _runner.RunAsync(_workflow, context, cancellationToken);

        return new QaSequentialResult
        {
            Question = question,
            RewrittenQuestion = context.RewrittenQuestion,
            DraftAnswer = context.RawAnswer,
            FinalAnswer = context.RefinedAnswer
        };
    }
}

public sealed class QaSequentialResult
{
    public string Question { get; set; } = string.Empty;
    public string? RewrittenQuestion { get; set; }
    public string? DraftAnswer { get; set; }
    public string? FinalAnswer { get; set; }
}

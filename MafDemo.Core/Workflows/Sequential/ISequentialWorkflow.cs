using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows.Sequential;

/// <summary>
/// 簡單的順序 Workflow：依序執行一組步驟
/// </summary>
public interface ISequentialWorkflow<TContext>
{
    string Name { get; }

    Task RunAsync(TContext context, CancellationToken cancellationToken = default);
}

using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Workflows;

/// <summary>
/// Workflow 中的一個步驟（節點 / Executor）
/// </summary>
/// <typeparam name="TContext">上下文資料模型，例如 QaSequentialContext</typeparam>
public interface IWorkflowStep<TContext>
{
    /// <summary>
    /// 步驟名稱，用於 logging / debug
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 執行此步驟，操作傳入的 context
    /// </summary>
    Task ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
}

using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Modes;

public interface IAppMode
{
    /// <summary>
    /// 模式代號，例如 "1"、"chat"、"seq"
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 顯示在選單上的名稱
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// 執行此模式的主流程
    /// </summary>
    Task RunAsync(CancellationToken cancellationToken = default);
}

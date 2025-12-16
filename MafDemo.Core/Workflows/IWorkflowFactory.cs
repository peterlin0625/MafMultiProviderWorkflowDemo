using MafDemo.Core.Modes;

namespace MafDemo.Core.Workflows;

public interface IWorkflowFactory
{
    /// <summary>
    /// 依 Id 或名稱取得對應模式（1 / 2 / 3 / 4 / 5 或 DisplayName）。
    /// </summary>
    IAppMode GetMode(string idOrName);

    /// <summary>
    /// 取得所有已註冊的模式，方便顯示主選單或 API 回傳清單。
    /// </summary>
    IReadOnlyCollection<IAppMode> GetAllModes();
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MafDemo.Core.Agents;

public interface IChatAgent
{
    /// <summary>
    /// Agent 名稱（可用來做 logging 或 routing）
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 給一個使用者輸入（可選擇帶上下文），讓 Agent 回覆一段文字。
    /// </summary>
    Task<string> RunAsync(
        string userInput,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 多輪版本，可以給完整對話歷史。
    /// （之後做 Groupchat / Handoffs 可以派上用場）
    /// </summary>
    Task<string> RunWithHistoryAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}

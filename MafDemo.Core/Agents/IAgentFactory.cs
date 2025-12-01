using MafDemo.Core.Llm;

namespace MafDemo.Core.Agents;

public interface IAgentFactory
{
    /// <summary>
    /// 依照 Provider 名稱建立一個對應的 Chat Agent。
    /// </summary>
    IChatAgent CreateChatAgent(string providerName, string? agentName = null);

    /// <summary>
    /// 使用預設 Provider 建立 Chat Agent。
    /// </summary>
    IChatAgent CreateDefaultChatAgent(string? agentName = null);
}

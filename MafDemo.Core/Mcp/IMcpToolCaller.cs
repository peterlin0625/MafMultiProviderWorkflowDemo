namespace MafDemo.Core.Mcp;

/// <summary>
/// MCP Tool 呼叫抽象介面：給 Workflow / Agent 使用，不關心底層是 HTTP、gRPC 或 stdio。
/// </summary>
public interface IMcpToolCaller
{
    /// <summary>
    /// 呼叫指定 MCP Tool，傳入 JSON 字串參數，回傳 JSON 字串結果。
    /// </summary>
    /// <param name="toolName">Tool 名稱，例如 "ToolA.CloudPrintProductSearch"</param>
    /// <param name="argumentsJson">參數 JSON 字串，例如 {"question":"...","category":"..."}</param>
    Task<string> CallToolAsync(string toolName, string argumentsJson, CancellationToken cancellationToken = default);
}

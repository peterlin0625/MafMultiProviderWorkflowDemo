using ModelContextProtocol.Server;

namespace CloudPrint.McpServer.Tools;

// 宣告這是一個 MCP 工具容器
[McpServerToolType]
public static class GetServerTimeTool
{
    // 實際的 Tool 方法
    [McpServerTool(Name = "getServerTime")]
    public static ServerTimeResult GetServerTime()
    {
        return new ServerTimeResult
        {
            ServerTime = DateTimeOffset.Now.ToString("O")
        };
    }

    public class ServerTimeResult
    {
        public string ServerTime { get; set; } = default!;
    }
}

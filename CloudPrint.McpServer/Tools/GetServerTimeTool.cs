using CloudPrint.McpServer.Observability;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CloudPrint.McpServer.Tools;

// 宣告這是一個 MCP 工具容器
[McpServerToolType]
public class GetServerTimeTool
{
    private readonly ToolCallContextAccessor _toolCall;
    private readonly ILogger<GetServerTimeTool> _logger;

    public GetServerTimeTool(
       ToolCallContextAccessor toolCall,
       ILogger<GetServerTimeTool> logger)
    {
        _toolCall = toolCall;
        _logger = logger;
    }


    // 實際的 Tool 方法
    [McpServerTool(Name = "getServerTime")]
    public ServerTimeResult GetServerTime()
    {
        var ctx = _toolCall.Current;

        _logger.LogInformation(
           "Executing tool. CorrelationId={CorrelationId}, ToolCallId={ToolCallId}",
           ctx?.CorrelationId,
           ctx?.ToolCallId);


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

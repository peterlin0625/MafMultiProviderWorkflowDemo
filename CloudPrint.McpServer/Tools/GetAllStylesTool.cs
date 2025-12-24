using CloudPrint.McpServer.DataApi;
using CloudPrint.McpServer.Observability;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CloudPrint.McpServer.Tools;

[McpServerToolType]
public class GetAllStylesTool
{
    private readonly StyleApiClient _api;
    private readonly ToolCallContextAccessor _toolCall;
    private readonly ILogger<GetAllStylesTool> _logger;

    public GetAllStylesTool(StyleApiClient api,
        ToolCallContextAccessor toolCall,
       ILogger<GetAllStylesTool> logger)
    {
        _api = api;
        _toolCall = toolCall;
        _logger = logger;
    }

    [McpServerTool(Name = "getAllStyles")]
    public async Task<object> GetAllStyles()
    {

        var ctx = _toolCall.Current;

        _logger.LogInformation(
           "Executing tool. CorrelationId={CorrelationId}, ToolCallId={ToolCallId}",
           ctx?.CorrelationId,
           ctx?.ToolCallId);

        return await _api.GetAllStylesAsync();
    }
}

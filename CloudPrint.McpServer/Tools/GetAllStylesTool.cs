using ModelContextProtocol.Server;
using CloudPrint.McpServer.DataApi;

namespace CloudPrint.McpServer.Tools;

[McpServerToolType]
public class GetAllStylesTool
{
    private readonly StyleApiClient _api;

    public GetAllStylesTool(StyleApiClient api)
    {
        _api = api;
    }

    [McpServerTool]
    public async Task<object> GetAllStyles()
    {
        return await _api.GetAllStylesAsync();
    }
}

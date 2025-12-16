using ModelContextProtocol.Server;
using CloudPrint.McpServer.DataApi;

namespace CloudPrint.McpServer.Tools;

[McpServerToolType]
public class GetStylesByGroupTool
{
    private readonly StyleApiClient _api;

    public GetStylesByGroupTool(StyleApiClient api)
    {
        _api = api;
    }

    [McpServerTool]
    public async Task<object> GetStylesByGroup(long groupId)
    {
        var all = await _api.GetAllStylesAsync();

        var group = all.FirstOrDefault(g => g.GroupId == groupId);

        if (group != null)
        {
            return group;
        }
        else
        {
            return new { Error = "Group not found" };
        }
    }
}

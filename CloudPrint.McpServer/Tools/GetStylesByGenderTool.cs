using ModelContextProtocol.Server;
using CloudPrint.McpServer.DataApi;

namespace CloudPrint.McpServer.Tools;

[McpServerToolType]
public class GetStylesByGenderTool
{
    private readonly StyleApiClient _api;

    public GetStylesByGenderTool(StyleApiClient api)
    {
        _api = api;
    }

    [McpServerTool]
    public async Task<object> GetStylesByGender(string gender)
    {
        var all = await _api.GetAllStylesAsync();

        var filtered = all
            .Where(g => g.Sex.Equals(gender, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return filtered;
    }
}

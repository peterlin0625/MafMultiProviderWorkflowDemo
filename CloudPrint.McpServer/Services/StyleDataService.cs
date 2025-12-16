using System.Net.Http.Json;
using CloudPrint.McpServer.Models;
using CloudPrint.McpServer.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CloudPrint.McpServer.Services;

public class StyleDataService
{
    private readonly HttpClient _http;
    private readonly StyleDataOptions _options;
    private readonly ILogger<StyleDataService> _logger;

    public StyleDataService(
        HttpClient http,
        IOptions<StyleDataOptions> options,
        ILogger<StyleDataService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<StyleGroupDto>> GetAllStylesAsync()
    {
        var url = $"{_options.BaseUrl}{_options.AllStylesEndpoint}";

        _logger.LogInformation("呼叫 Data API：{Url}", url);

        var result = await _http.GetFromJsonAsync<List<StyleGroupDto>>(url);

        return result ?? new List<StyleGroupDto>();
    }
}

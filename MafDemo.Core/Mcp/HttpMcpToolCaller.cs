using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MafDemo.Core.Mcp;

public sealed class McpOptions
{
    public string BaseUrl { get; set; } = "https://localhost:5005"; // 你的 MCP Server 位址
    public string ApiKey { get; set; } = string.Empty;              // 若有需要可加授權
}

public sealed class HttpMcpToolCaller : IMcpToolCaller
{
    private readonly HttpClient _httpClient;
    private readonly McpOptions _options;
    private readonly ILogger<HttpMcpToolCaller> _logger;

    public HttpMcpToolCaller(
        HttpClient httpClient,
        IOptions<McpOptions> options,
        ILogger<HttpMcpToolCaller> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<string> CallToolAsync(
        string toolName,
        string argumentsJson,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("HttpMcpToolCaller: 呼叫 Tool = {ToolName}, Args = {Args}",
            toolName, argumentsJson);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/tools/call");

        // 這裡的 payload schema 可以依你的 MCP Server 設計調整
        var payload = new
        {
            tool = toolName,
            arguments = JsonDocument.Parse(argumentsJson).RootElement
        };

        request.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            request.Headers.Add("X-API-Key", _options.ApiKey);
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        _logger.LogInformation("HttpMcpToolCaller: Tool {ToolName} 回應 = {Content}",
            toolName, content);

        return content;
    }
}

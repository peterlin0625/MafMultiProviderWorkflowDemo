using CloudPrint.McpServer.Models;
using MafDemo.McpServer.Options;
using MafDemo.McpServer.Security;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CloudPrint.McpServer.DataApi;

public sealed class StyleApiClient
{
    private readonly HttpClient _http;
    private readonly HmacSigner _signer;

    // Use IHttpClientFactory so the service can be resolved even when a typed
    // AddHttpClient<StyleApiClient> registration is not present.
    public StyleApiClient(
        IHttpClientFactory httpFactory,
        IOptions<DataApiOptions> options)
    {
        var opt = options.Value;

        _http = httpFactory.CreateClient();
        if (!string.IsNullOrEmpty(opt.BaseUrl))
            _http.BaseAddress = new Uri(opt.BaseUrl);

        _signer = new HmacSigner(opt.ApiKey, opt.ApiSecret);
    }
    /// <summary>
    /// 呼叫唯一資料來源：GET /api/styles/all
    /// </summary>
    public async Task<List<StyleGroupDto>> GetAllStylesAsync(
        CancellationToken cancellationToken = default)
    {
        const string path = "/styles/all";

        // GET 沒有 body
        var sig = _signer.CreateSignature(
            httpMethod: "GET",
            path: path,
            body: "");

        using var request = new HttpRequestMessage(HttpMethod.Get, path);

        // === HMAC Headers ===
        request.Headers.Add("X-Api-Key", sig.ApiKey);
        request.Headers.Add("X-Timestamp", sig.Timestamp);
        request.Headers.Add("X-Signature", sig.Signature);

        using var response = await _http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result =
            await response.Content.ReadFromJsonAsync<List<StyleGroupDto>>(cancellationToken: cancellationToken);

        return result ?? new List<StyleGroupDto>();
    }



    /// Tool 呼叫時（LLM → MCP） 
    /// {
    ///  "tool_call_id": "call_9f3a8d",
    ///  "arguments": {
    ///    "productId": 123,
    ///    "qty": 1
    ///  }
    /// }
    /// <summary>
    /// POST / Side-effect API
    /// Nonce = tool_call_id 的完整實作示例
    /// </summary>
    /// <param name="body">物件 CreateOrderRequest </param>
    /// <param name="toolCallId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task CreateOrderAsync(
    Object body,
    string toolCallId,
    CancellationToken ct = default)
    {
        const string path = "/api/orders";

        var json = JsonSerializer.Serialize(body);

        var sig = _signer.CreateSignature(
            httpMethod: "POST",
            path: path,
            body: json);

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("X-Api-Key", sig.ApiKey);
        request.Headers.Add("X-Timestamp", sig.Timestamp);
        request.Headers.Add("X-Signature", sig.Signature);

        // ⭐ 關鍵：Nonce = tool_call_id
        request.Headers.Add("X-Nonce", toolCallId);

        using var response = await _http.SendAsync(request, ct);

        response.EnsureSuccessStatusCode();
    }
}

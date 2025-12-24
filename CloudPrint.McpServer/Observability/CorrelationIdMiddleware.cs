using CloudPrint.McpServer.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CloudPrint.McpServer.Observability;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1️⃣ 讀取 Client 傳入的 CorrelationId
        var correlationId =
            context.Request.Headers.TryGetValue(HeaderName, out var values)
                && !string.IsNullOrWhiteSpace(values.FirstOrDefault())
                    ? values.First()!
                    : Guid.NewGuid().ToString("N");

        // 2️⃣ 寫回 Response Header（方便 debug）
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // 3️⃣ 放入 AsyncLocal（整個 request 共用）
        using var _ = CorrelationContext.Begin(correlationId);

        // 4️⃣ 建立 ILogger Scope
        using var _scope = CorrelationLoggingScope.Begin(_logger);

        await _next(context);
    }
}

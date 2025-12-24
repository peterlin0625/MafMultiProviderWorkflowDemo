using CloudPrint.McpServer.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CloudPrint.McpServer.Middleware;

public sealed class ToolCallContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ToolCallContextAccessor _accessor;
    private readonly ILogger<ToolCallContextMiddleware> _logger;

    public ToolCallContextMiddleware(
        RequestDelegate next,
        ToolCallContextAccessor accessor,
        ILogger<ToolCallContextMiddleware> logger)
    {
        _next = next;
        _accessor = accessor;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // MCP Tool Call 一定是 POST
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // 允許讀取 body 兩次
        context.Request.EnableBuffering();

        string? toolCallId = null;

        try
        {
            using var doc = await JsonDocument.ParseAsync(
                context.Request.Body,
                cancellationToken: context.RequestAborted);

            // MCP tool call id（依實際 MCP payload key 調整）
            toolCallId =
                doc.RootElement.TryGetProperty("id", out var id)
                    ? id.GetString()
                    : null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to parse MCP request body.");
        }
        finally
        {
            context.Request.Body.Position = 0;
        }

        if (!string.IsNullOrWhiteSpace(toolCallId))
        {
            var correlationId = CorrelationContext.Current
                ?? Guid.NewGuid().ToString("N");

            using var _ = _accessor.Begin(
                new ToolCallContext(toolCallId, correlationId));

            await _next(context);
            return;
        }

        await _next(context);
    }
}

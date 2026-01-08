using CloudPrint.McpServer.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;
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
       
        var toolCallId =
        context.Request.Headers["X-Tool-Call-Id"].FirstOrDefault();

        // fallback：MCP JSON body
        if (string.IsNullOrWhiteSpace(toolCallId)
            && HttpMethods.IsPost(context.Request.Method))
        {
            context.Request.EnableBuffering();
            try
            {
                using var doc = await JsonDocument.ParseAsync(
                    context.Request.Body,
                    cancellationToken: context.RequestAborted);

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
        }

        if (!string.IsNullOrWhiteSpace(toolCallId))
        {
            var correlationId =
                context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");

            using (LogContext.PushProperty("toolCallId", toolCallId))
            using (_accessor.Begin(new ToolCallContext(toolCallId, correlationId)))
            {
                await _next(context);
                return;
            }
        }

        await _next(context);
    }
}

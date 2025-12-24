using CloudPrint.McpServer.Audit;
using CloudPrint.McpServer.Idempotency;
using CloudPrint.McpServer.Observability;
using CloudPrint.McpServer.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Middleware;

public sealed class ToolExecutionMiddleware
{
    private readonly RequestDelegate _next;

    public ToolExecutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ToolCallContextAccessor toolCallAccessor,
        IToolExecutionAuditSink audit,
        IToolExecutionIdempotencyStore idempotency,
        ILogger<ToolExecutionMiddleware> logger)
    {
        var toolCtx = toolCallAccessor.Current;

        if (toolCtx is null)
        {
            await _next(context);
            return;
        }

        var toolCallId = toolCtx.ToolCallId;
        var correlationId = toolCtx.CorrelationId;

        // 3️⃣ Idempotency
        if (!await idempotency.TryBeginAsync(toolCallId))
        {
            logger.LogWarning(
                "Duplicate tool execution blocked. ToolCallId={ToolCallId}",
                toolCallId);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            return;
        }

        using var activity =
            ToolExecutionActivitySource.Instance.StartActivity("tool.execute");

        activity?.SetTag("correlation.id", correlationId);
        activity?.SetTag("tool.call_id", toolCallId);

        await audit.WriteAsync(new ToolExecutionAuditEvent
        {
            CorrelationId = correlationId,
            ToolCallId = toolCallId,
            Phase = "Start"
        });

        try
        {
            await _next(context);

            await audit.WriteAsync(new ToolExecutionAuditEvent
            {
                CorrelationId = correlationId,
                ToolCallId = toolCallId,
                Phase = "Success"
            });
        }
        catch (Exception ex)
        {
            await audit.WriteAsync(new ToolExecutionAuditEvent
            {
                CorrelationId = correlationId,
                ToolCallId = toolCallId,
                Phase = "Failed",
                Payload = ex.Message
            });

            throw;
        }
        finally
        {
            await idempotency.CompleteAsync(toolCallId);
        }
    }
}

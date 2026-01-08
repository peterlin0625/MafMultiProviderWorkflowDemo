using System.Net.Http;
using MafDemo.McpClientApp.Observability;

namespace MafDemo.McpClientApp.Observability;

public sealed class CorrelationIdHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = CorrelationContext.Current;

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            request.Headers.Remove("X-Correlation-Id");
            request.Headers.Add("X-Correlation-Id", correlationId); 
        }


        // === ToolCallId ===
        if (ToolCallContextAccessor.Current is { } ctx)
        {
            request.Headers.Remove("X-Tool-Call-Id");
            request.Headers.Add("X-Tool-Call-Id", ctx.ToolCallId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

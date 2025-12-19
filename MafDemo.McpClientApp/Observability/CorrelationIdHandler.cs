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

        return base.SendAsync(request, cancellationToken);
    }
}

using MafDemo.McpClientApp.Adapters;
using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Policies;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Runtime;

public sealed class ToolInvoker
{
    private readonly IToolClient _toolClient;
    private readonly RetryPolicy _retryPolicy;

    public ToolInvoker(IToolClient toolClient, RetryPolicy retryPolicy)
    {
        _toolClient = toolClient;
        _retryPolicy = retryPolicy;
    }

    public async Task<object> InvokeAsync(
    ToolCallContext context,
    IReadOnlyDictionary<string, object?> arguments,
    CancellationToken ct)
    {
        while (true)
        {
            try
            {
                context.MarkAttempt();

                var result = await _toolClient.CallAsync(
                    context.ToolName,
                    arguments,
                    context.ToolCallId,
                    ct);

                context.MarkSuccess();
                return result;
            }
            catch (Exception ex)
            {
                context.MarkFailure(ex);

                if (!_retryPolicy.CanRetry(context))
                {
                    context.Abort();
                    throw;
                }
            }
        }
    }

}


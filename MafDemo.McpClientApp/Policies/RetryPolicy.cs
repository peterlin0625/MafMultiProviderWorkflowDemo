using MafDemo.McpClientApp.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Policies;

public sealed class RetryPolicy
{
    public int MaxAttempts { get; }

    public RetryPolicy(int maxAttempts)
    {
        MaxAttempts = maxAttempts;
    }

    public bool CanRetry(ToolCallContext context)
    {
        if (!context.IsSideEffect)
            return context.AttemptCount < MaxAttempts;

        // side-effect：只允許有限 retry
        return context.IdempotencyExpected
               && context.AttemptCount < MaxAttempts;
    }
}


using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.Concurrent;

namespace CloudPrint.McpServer.Idempotency;

public sealed class InMemoryToolExecutionIdempotencyStore
    : IToolExecutionIdempotencyStore
{
    private readonly ConcurrentDictionary<string, byte> _executing = new();

    public Task<bool> TryBeginAsync(string toolCallId)
        => Task.FromResult(_executing.TryAdd(toolCallId, 0));

    public Task CompleteAsync(string toolCallId)
    {
        _executing.TryRemove(toolCallId, out _);
        return Task.CompletedTask;
    }
}

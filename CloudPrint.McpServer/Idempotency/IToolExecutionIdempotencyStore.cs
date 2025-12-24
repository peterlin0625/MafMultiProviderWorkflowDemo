using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Idempotency;

public interface IToolExecutionIdempotencyStore
{
    Task<bool> TryBeginAsync(string toolCallId);
    Task CompleteAsync(string toolCallId);
}

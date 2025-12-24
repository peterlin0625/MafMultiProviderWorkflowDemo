using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Observability;

/// <summary>
/// Static bridge to expose ToolCallContext to Serilog enrichers.
/// </summary>
public static class ToolCallContextAccessorHolder
{
    public static ToolCallContextAccessor? Current { get; set; }
}

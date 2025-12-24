
using System.Diagnostics;

namespace CloudPrint.McpServer.Telemetry;

public static class ToolExecutionActivitySource
{
    public static readonly ActivitySource Instance =
        new("CloudPrint.McpServer.ToolExecution");
}

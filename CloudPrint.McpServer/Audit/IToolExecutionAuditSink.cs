using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Audit;

public interface IToolExecutionAuditSink
{
    Task WriteAsync(ToolExecutionAuditEvent evt);
}

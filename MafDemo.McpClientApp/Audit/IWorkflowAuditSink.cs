using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Audit;
public interface IWorkflowAuditSink
{
    Task WriteAsync(WorkflowAuditEvent evt);
}

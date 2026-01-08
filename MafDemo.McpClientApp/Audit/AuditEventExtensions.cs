using Serilog;

namespace MafDemo.McpClientApp.Audit;

public static class AuditEventExtensions
{
    public static void WriteToSerilog(
        this AuditEvent evt,
        ILogger logger)
    {
        logger
            .ForContext("correlationId", evt.CorrelationId)
            .ForContext("subjectType", evt.SubjectType.ToString())
            .ForContext("subjectName", evt.SubjectName)
            .ForContext("phase", evt.Phase)
            .ForContext("payload", evt.Payload, destructureObjects: true)
            .Information("AuditEvent");
    }
}

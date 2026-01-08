using Serilog;

namespace MafDemo.McpClientApp.Audit;

/// <summary>
/// 將 AuditEvent 寫入 Serilog（Console / Elastic / 其他 Sink）
/// </summary>
public sealed class SerilogAuditSink : IAuditSink
{
    private readonly ILogger _logger;

    public SerilogAuditSink()
    {
        // 使用全域 Serilog logger
        _logger = Log.Logger;
    }

    public Task WriteAsync(AuditEvent evt)
    {
        evt.WriteToSerilog(_logger);
        return Task.CompletedTask;
    }
}

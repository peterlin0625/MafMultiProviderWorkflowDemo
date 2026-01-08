using Serilog.Context;

namespace MafDemo.McpClientApp.Observability;

/// <summary>
/// 將 CorrelationContext.Current 推入 Serilog LogContext
/// 設計重點
//  不自己產生 CorrelationId
//  只讀 CorrelationContext.Current
//  只負責 enrich
//  Dispose 時自動清掉
/// </summary>
public sealed class CorrelationLoggingScope : IDisposable
{
    private readonly IDisposable? _scope;

    public CorrelationLoggingScope()
    {
        var correlationId = CorrelationContext.Current;

        if (!string.IsNullOrEmpty(correlationId))
        {
            _scope = LogContext.PushProperty(
                "correlationId",
                correlationId);
        }
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}

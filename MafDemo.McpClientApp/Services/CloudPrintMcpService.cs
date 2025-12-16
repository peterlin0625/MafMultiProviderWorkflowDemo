using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol;
using MafDemo.McpClientApp.Options;

namespace MafDemo.McpClientApp.Services;

public class CloudPrintMcpService
{
    private readonly CloudPrintMcpClientOptions _options;
    private readonly ILogger<CloudPrintMcpService> _logger;
    private readonly McpClient _innerClient;

    public CloudPrintMcpService(
        IOptions<CloudPrintMcpClientOptions> opt,
        ILogger<CloudPrintMcpService> logger)
    {
        _options = opt.Value;
        _logger = logger;

        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(_options.Endpoint)
        });

        _innerClient = McpClient.CreateAsync(transport).GetAwaiter().GetResult();
    }

    public async Task<object?> CallToolAsync(
        string toolName,
        IReadOnlyDictionary<string, object?>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("呼叫 MCP Tool: {Tool}", toolName);

        var retries = _options.RetryCount + 1;

        for (int i = 1; i <= retries; i++)
        {
            try
            {
                var result = await _innerClient.CallToolAsync(
                        toolName: toolName,
                        arguments: arguments,
                        progress: null,
                        options: null,
                        cancellationToken: cancellationToken);

                _logger.LogInformation("MCP Tool {Tool} 呼叫成功。", toolName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MCP Tool 呼叫失敗，第 {Try}/{Max}", i, retries);
                if (i == retries)
                    throw;
                await Task.Delay(_options.TimeoutSeconds, cancellationToken);
            }
        }

        return null;
    }
}

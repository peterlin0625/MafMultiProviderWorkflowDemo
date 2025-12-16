namespace MafDemo.McpClientApp.Options;

public class CloudPrintMcpClientOptions
{
    public string Endpoint { get; set; } = "";
    public int RetryCount { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 30;
}

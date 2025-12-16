using MafDemo.McpClientApp.Services;

namespace MafDemo.McpClientApp.Workflows;

public class TestWorkflow
{
    private readonly CloudPrintMcpService _mcp;

    public TestWorkflow(CloudPrintMcpService mcp)
    {
        _mcp = mcp;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== MCP Client Workflow Test ===");

        var args = new Dictionary<string, object?>
        {
            ["productId"] = "P12345"
        };

        var result = await _mcp.CallToolAsync("getProductInfo", args);

        Console.WriteLine("=== MCP Tool Result ===");
        Console.WriteLine(result);
    }
}

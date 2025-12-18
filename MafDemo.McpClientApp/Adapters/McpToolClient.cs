using ModelContextProtocol.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Adapters; 
  
public sealed class McpToolClient : IToolClient
{
    private readonly McpClient _client;

    public McpToolClient(McpClient client)
    {
        _client = client;
    }

    public async Task<object> CallAsync(
        string toolName,
        IReadOnlyDictionary<string, object?> arguments,
        string toolCallId,
        CancellationToken cancellationToken)
    {
        return await _client.CallToolAsync(
            toolName: toolName,
            arguments: arguments,
            progress: null,
            options: null,
            cancellationToken: cancellationToken
        );
    }
}

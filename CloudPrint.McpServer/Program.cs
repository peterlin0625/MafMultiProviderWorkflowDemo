using CloudPrint.McpServer.DataApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// 註冊 HttpClient → 指向 Data API（部署在 IIS）
builder.Services.AddHttpClient<StyleApiClient>(client =>
{
    client.BaseAddress = new Uri("https://cloudprintapi.company.com");
});

// 註冊所有 MCP Tools
builder.Services
    .AddMcpServer()
    .WithToolsFromAssembly()
    .WithHttpTransport();

var app = builder.Build();

// MCP Endpoint: /mcp
app.MapMcp("/mcp");

app.Run();

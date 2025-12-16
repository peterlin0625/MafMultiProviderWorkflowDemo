using CloudPrint.McpServer.DataApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7282, listenOptions =>
    {
        listenOptions.UseHttps(); // 開發環境會使用 dev cert；生產請指定 PFX
    });
});

var app = builder.Build();

// MCP Endpoint: /mcp
app.MapMcp("/mcp");

app.Run();

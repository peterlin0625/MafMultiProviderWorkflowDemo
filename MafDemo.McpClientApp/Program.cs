using MafDemo.McpClientApp.Adapters;
using MafDemo.McpClientApp.Options;
using MafDemo.McpClientApp.Policies;
using MafDemo.McpClientApp.Runtime;
using MafDemo.McpClientApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// 加 appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// DI 註冊
builder.Services.AddMcpClientServices(builder.Configuration);

// Host
using IHost host = builder.Build(); 

// 4) 執行 Workflow 測試
//var workflow = host.Services.GetRequiredService<TestWorkflow>();
//await workflow.RunAsync();

await host.RunAsync();

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MafDemo.McpClientApp.Services;
using MafDemo.McpClientApp.Workflows;
using MafDemo.McpClientApp.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// 1) 加 appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 2) DI 註冊
builder.Services.AddMcpClientServices(builder.Configuration); 

// 3) Host
using IHost host = builder.Build();

// 4) 執行 Workflow 測試
var workflow = host.Services.GetRequiredService<TestWorkflow>();
await workflow.RunAsync();

await host.RunAsync();

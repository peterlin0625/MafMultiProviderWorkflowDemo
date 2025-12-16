using MafDemo.Core;
using MafDemo.Core.Llm;
using MafDemo.Core.Modes;
using MafDemo.Core.Workflows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// 建立啟動期 Logger（只用 Console），避免啟動錯誤沒 log
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();


var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .UseSerilog((context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)  // 讀取 appsettings.json 中的 Serilog section
            .ReadFrom.Services(services)                    // 讓 ILogger<T> 內的 DI 結構可用來 enrich
            .Enrich.FromLogContext();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // LLM Providers + Agents + Workflows + Modes
        services.AddLlmProviders(configuration);
    });

var host = builder.Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

 
var factory = host.Services.GetRequiredService<IWorkflowFactory>();
var llmOptions = host.Services.GetRequiredService<IOptions<LlmOptions>>().Value;


while (true)
{
    Console.WriteLine("=== .NET 10 + MAF Agent + Multi-Provider LLM Demo ===");
    Console.WriteLine($"目前 Default Provider：{llmOptions.DefaultProvider}");
    Console.WriteLine();

    Console.WriteLine("請選擇模式（輸入編號）：");

    var modes = factory.GetAllModes().ToList();
    foreach (var mode in modes)
    {
        Console.WriteLine($"  {mode.Id}. {mode.DisplayName}");
    }
    Console.WriteLine("  q. 離開");
    Console.WriteLine();

    Console.Write("你的選擇：");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) ||
        string.Equals(input.Trim(), "q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("再見～");
        break;
    }

    try
    {
        var mode = factory.GetMode(input.Trim());
        await mode.RunAsync(CancellationToken.None);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"選擇模式時發生錯誤：{ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("按任意鍵回到主選單...");
    Console.ReadKey();
    Console.Clear();
}

Log.CloseAndFlush();
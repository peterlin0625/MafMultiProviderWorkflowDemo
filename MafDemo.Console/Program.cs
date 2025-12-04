using MafDemo.Core;
using MafDemo.Core.Modes;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

var modes = services.GetRequiredService<IEnumerable<IAppMode>>()
                    .OrderBy(m => m.Id)
                    .ToList();

if (!modes.Any())
{
    Console.WriteLine("沒有可用的模式（IAppMode）。請確認 DI 註冊。");
    return;
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== MafMultiProviderWorkflowDemo ===");
    Console.WriteLine("請選擇要執行的模式：");
    foreach (var mode in modes)
    {
        Console.WriteLine($"  {mode.Id}. {mode.DisplayName}");
    }
    Console.WriteLine("  q. 離開");
    Console.Write("請輸入選項：");

    var choice = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(choice))
        continue;

    if (string.Equals(choice.Trim(), "q", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Bye ~");
        break;
    }

    var selected = modes.FirstOrDefault(m =>
        string.Equals(m.Id, choice.Trim(), StringComparison.OrdinalIgnoreCase));

    if (selected == null)
    {
        Console.WriteLine("無效選項，請重新輸入。");
        continue;
    }

    Console.WriteLine();
    Console.WriteLine($"== 執行 {selected.DisplayName} ==");

    try
    {
        await selected.RunAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"執行模式時發生錯誤：{ex.Message}");
    }
 
}

Log.CloseAndFlush();
using MafDemo.Core;
using MafDemo.Core.Agents;
using MafDemo.Core.Llm;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // 註冊 LLM Providers + Agent 工廠
        services.AddLlmProviders(configuration);

        // 後續會在這裡再加上 MAF Workflows 相關註冊
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });

var host = builder.Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;


//// 這裡先看一下 Options
//var llmOptions = services.GetRequiredService<IOptions<LlmOptions>>().Value;

//Console.WriteLine("=== Debug LlmOptions ===");
//Console.WriteLine($"DefaultProvider = {llmOptions.DefaultProvider}");
//Console.WriteLine($"Providers Count = {llmOptions.Providers.Count}");
//foreach (var kvp in llmOptions.Providers)
//{
//    Console.WriteLine($"Provider: {kvp.Key}, BaseUrl={kvp.Value.BaseUrl}, Model={kvp.Value.Model}");
//}
//Console.WriteLine("========================");

var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<LlmOptions>>().Value;
var agentFactory = services.GetRequiredService<IAgentFactory>();

Console.WriteLine("=== Multi-Provider + MafLlmChatAgent 測試 ===");
Console.WriteLine("已設定的 Providers:");

foreach (var kvp in options.Providers)
{
    Console.WriteLine($" - {kvp.Key}");
}

Console.WriteLine();
Console.WriteLine($"預設 Provider: {options.DefaultProvider}");
Console.Write("輸入要使用的 Provider 名稱（直接 Enter 使用預設）：");

var providerInput = Console.ReadLine();
var providerName = string.IsNullOrWhiteSpace(providerInput)
    ? options.DefaultProvider
    : providerInput.Trim();

IChatAgent agent;

try
{
    if (string.Equals(providerName, options.DefaultProvider, StringComparison.OrdinalIgnoreCase))
    {
        agent = agentFactory.CreateDefaultChatAgent();
    }
    else
    {
        agent = agentFactory.CreateChatAgent(providerName);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"建立 Agent 失敗：{ex.Message}");
    return;
}

Console.WriteLine();
Console.WriteLine($"使用 Provider：{providerName}，Agent：{agent.Name}");
Console.Write("請輸入你的問題：");

var question = Console.ReadLine();

if (string.IsNullOrWhiteSpace(question))
{
    Console.WriteLine("未輸入問題，程式結束。");
    return;
}

try
{
    var answer = await agent.RunAsync(question!);

    Console.WriteLine();
    Console.WriteLine("=== Agent 回覆 ===");
    Console.WriteLine(answer);
    Console.WriteLine("==================");
}
catch (NotImplementedException nie)
{
    Console.WriteLine($"這個 Provider 尚未實作完整呼叫邏輯：{nie.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"呼叫 Agent 時發生錯誤：{ex.Message}");
}

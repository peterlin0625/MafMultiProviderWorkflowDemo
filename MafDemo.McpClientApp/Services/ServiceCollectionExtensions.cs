using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MafDemo.McpClientApp.Options;
using MafDemo.McpClientApp.Services;
using MafDemo.McpClientApp.Workflows;

namespace MafDemo.McpClientApp.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMcpClientServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // 正確的 options 設定用法 ↓↓↓
        services.Configure<CloudPrintMcpClientOptions>(
            config.GetSection("CloudPrint:McpClient"));

        services.AddSingleton<CloudPrintMcpService>();

        services.AddSingleton<TestWorkflow>();

        return services;
    } 
}

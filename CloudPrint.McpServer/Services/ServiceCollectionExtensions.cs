using CloudPrint.McpServer.DataApi;
using CloudPrint.McpServer.Options;
using MafDemo.McpServer.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudPrint.McpServer.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStyleServices(
        this IServiceCollection services,
        IConfiguration config)
    { 
        // 註冊 HttpClient → 指向 Data API（從 appsettings.json 讀取）
        services.Configure<DataApiOptions>(config.GetSection("DataApi"));
        // register a generic HttpClient factory (typed registration was removed)
        services.AddHttpClient();

        // Register StyleApiClient so it can be injected into MCP tools
        services.AddTransient<StyleApiClient>();
        return services;
    }
}

using CloudPrint.McpServer.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudPrint.McpServer.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStyleServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<StyleDataOptions>(config.GetSection("StyleData"));
        services.AddHttpClient<StyleDataService>();
        return services;
    }
}

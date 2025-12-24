using CloudPrint.McpServer.Observability.Serilog;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace CloudPrint.McpServer.Logging;

public static class SerilogExtensions
{
    public static void ConfigureSerilog(
        this LoggerConfiguration cfg)
    {
        cfg
            .Enrich.FromLogContext()
            .Enrich.With(new CorrelationIdEnricher())
            .Enrich.With(new ToolCallIdEnricher())
            .WriteTo.Console();
    }


    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            // Elastic（正式環境）
            //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
            //    new Uri("http://localhost:9200"))
            //{
            //    IndexFormat = "cloudprint-mcp-audit-{0:yyyy.MM.dd}"
            //})
            .CreateLogger();


        builder.Host.UseSerilog((ctx, services, cfg) =>
        {
            cfg
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With(new CorrelationIdEnricher())
                .Enrich.With(new ToolCallIdEnricher())
                .WriteTo.Console();
        });
    }
}

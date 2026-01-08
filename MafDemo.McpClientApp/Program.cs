using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
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
using Serilog;

 
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// 加 appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

var elasticEndpoint = builder.Configuration["Elastic:Endpoint"]
    ?? throw new InvalidOperationException("Elastic:Endpoint is not configured");

 
// ✅ Serilog wiring（Generic Host 正確方式）
builder.Services.AddSerilog((services, cfg) =>
{
    cfg
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();

    var endpoint = builder.Configuration["Elastic:Endpoint"];
    if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri))
    {
        cfg.WriteTo.Elasticsearch(
            new[] { uri },
            opts => opts.DataStream = new DataStreamName("cloudprint", "audit", "client"));
    }
});

// DI 註冊
builder.Services.AddMcpClientServices(builder.Configuration);

// Host
using IHost host = builder.Build();  

await host.RunAsync();

using MafDemo.McpClientApp.Adapters;
using MafDemo.McpClientApp.Agents;
using MafDemo.McpClientApp.Audit;
using MafDemo.McpClientApp.HumanInLoop;
using MafDemo.McpClientApp.Llm;
using MafDemo.McpClientApp.Observability;
using MafDemo.McpClientApp.Options;
using MafDemo.McpClientApp.Policies;
using MafDemo.McpClientApp.Runtime;
using MafDemo.McpClientApp.Services;
using MafDemo.McpClientApp.Workflows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace MafDemo.McpClientApp.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMcpClientServices(
        this IServiceCollection services,
        IConfiguration config)
    { 

        services.Configure<CloudPrintMcpClientOptions>(
            config.GetSection("CloudPrint:McpClient"));

        services.AddHttpClient("McpClient")
            .AddHttpMessageHandler<CorrelationIdHandler>();

        // MCP Client  
        services.AddSingleton<McpClient>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient("McpClient");

            var options = sp
                .GetRequiredService<IOptions<CloudPrintMcpClientOptions>>()
                .Value;

            var transport = new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(options.Endpoint)
                },
                httpClient);

            return McpClient
                .CreateAsync(transport)
                .GetAwaiter()
                .GetResult();
        }); 

        // Adapter
        services.AddSingleton<IToolClient, McpToolClient>();

        // Policy
        services.AddSingleton(new RetryPolicy(maxAttempts: 3));

        // Runtime
        services.AddSingleton<ToolInvoker>();


        // Workflow factory（每次執行一個）
        services.AddTransient<IWorkflowDefinition, GetServerTimeWorkflow>();
        services.AddTransient<IWorkflowDefinition, CreatePrintJobWorkflow>();

        // LLM options
        services.Configure<LlmOptions>(
            config.GetSection("Llm"));

        // LLM HttpClient
        services.AddHttpClient<ILlmClient, OpenAiLlmClient>();


        // Fallback
        services.AddSingleton<AgentFallbackPolicy>();

        // Audit
        services.AddSingleton<IWorkflowDecisionStore,
            InMemoryWorkflowDecisionStore>();

        // Human-in-the-loop
        services.AddSingleton<IUserConfirmationService,
            ConsoleUserConfirmationService>();


        services.AddTransient<CorrelationIdHandler>();

        // Agent
        services.AddSingleton<CloudPrintAgent>();

        return services;
    } 
}

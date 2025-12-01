using System;
using MafDemo.Core.Llm;
using MafDemo.Core.Agents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MafDemo.Core;

public static class LlmServiceCollectionExtensions
{
    public static IServiceCollection AddLlmProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LlmOptions>(configuration.GetSection("Llm"));

        services.AddSingleton<ILlmProviderFactory, LlmProviderFactory>();

        services.AddHttpClient<MistralLlmProvider>();
        services.AddSingleton<ILlmProvider, MistralLlmProvider>();

        services.AddSingleton<ILlmProvider, OpenAiLlmProvider>();
        services.AddSingleton<ILlmProvider, OllamaLlmProvider>();
        services.AddSingleton<ILlmProvider, GeminiLlmProvider>();

        // 🆕 Agent Factory
        services.AddSingleton<IAgentFactory, AgentFactory>();

        return services;
    }
}

using MafDemo.Core.Llm;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Agents;

public sealed class AgentFactory : IAgentFactory
{
    private readonly ILlmProviderFactory _providerFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly PromptOptions _promptOptions;

    public AgentFactory(
        ILlmProviderFactory providerFactory,
        ILoggerFactory loggerFactory,
        PromptOptions promptOptions)
    {
        _providerFactory = providerFactory;
        _loggerFactory = loggerFactory;
        _promptOptions = promptOptions;
    }

    public IChatAgent CreateChatAgent(string providerName, string? agentName = null)
    {
        var provider = _providerFactory.GetProvider(providerName);
        var logger = _loggerFactory.CreateLogger<MafLlmChatAgent>();

        return new MafLlmChatAgent(provider, logger, _promptOptions, agentName);
    }

    public IChatAgent CreateDefaultChatAgent(string? agentName = null)
    {
        var provider = _providerFactory.GetDefaultProvider();
        var logger = _loggerFactory.CreateLogger<MafLlmChatAgent>();

        return new MafLlmChatAgent(provider, logger, _promptOptions, agentName);
    }
}

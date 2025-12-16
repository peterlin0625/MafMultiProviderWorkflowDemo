using MafDemo.Core.Agents;
using MafDemo.Core.Execution;
using MafDemo.Core.Json;
using MafDemo.Core.Llm;
using MafDemo.Core.Mcp;
using MafDemo.Core.Modes;
using MafDemo.Core.Repository;
using MafDemo.Core.Workflows;
using MafDemo.Core.Workflows.Concurrent;
using MafDemo.Core.Workflows.Groupchat;
using MafDemo.Core.Workflows.Handoffs;
using MafDemo.Core.Workflows.Sequential;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;


namespace MafDemo.Core;

public static class LlmServiceCollectionExtensions
{
    public static IServiceCollection AddLlmProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LlmOptions>(configuration.GetSection("Llm"));

        // === 把 PromptOptions 綁進來 ===
        services.AddSingleton<PromptOptions>(sp =>
        {
            var env = sp.GetRequiredService<IHostEnvironment>();
            var promptOptions = new PromptOptions();

            // 從 appsettings.json 讀取 markdown 路徑
            var promptPath = configuration["Prompts:DefaultSystemPromptPath"];

            if (!string.IsNullOrWhiteSpace(promptPath))
            {
                // 相對於 ContentRoot（通常是專案輸出目錄）
                var fullPath = Path.Combine(env.ContentRootPath, promptPath);

                if (File.Exists(fullPath))
                {
                    promptOptions.DefaultSystemPrompt = File.ReadAllText(fullPath);
                }
            }

            // 如果沒設定檔或讀檔失敗，就 fallback 成原本那段內建字串
            if (string.IsNullOrWhiteSpace(promptOptions.DefaultSystemPrompt))
            {
                promptOptions.DefaultSystemPrompt =
                    "你是一個熟悉 .NET 10、MAF (Microsoft Agent Framework)、MCP、" +
                    "以及多雲 LLM Provider 的技術顧問，回答請用繁體中文，" +
                    "保持專業但語氣友善、簡潔。";
            }

            return promptOptions;
        });

        services.AddSingleton<LlmJsonSanitizer>();

        services.AddSingleton<ILlmProviderFactory, LlmProviderFactory>();

        services.AddHttpClient<MistralLlmProvider>();

        services.AddSingleton<ILlmProvider, MistralLlmProvider>();
        services.AddSingleton<ILlmProvider, OpenAiLlmProvider>();
        services.AddSingleton<ILlmProvider, OllamaLlmProvider>();
        services.AddSingleton<ILlmProvider, GeminiLlmProvider>();

        // 🆕 Agent Factory
        services.AddSingleton<IAgentFactory, AgentFactory>();

        // 🆕 Workflow Runner
        services.AddSingleton<IWorkflowRunner, WorkflowRunner>();

        #region Sequential 
        // 🆕 QA Sequential Workflow + Steps
        services.AddTransient<RewriteQuestionStep>();
        services.AddTransient<AnswerQuestionStep>();
        services.AddTransient<RefineAnswerStep>();

        services.AddTransient<ISequentialWorkflow<QaSequentialContext>>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<QaSequentialWorkflow>>();
            var steps = new IWorkflowStep<QaSequentialContext>[]
            {
                sp.GetRequiredService<RewriteQuestionStep>(),
                sp.GetRequiredService<AnswerQuestionStep>(),
                sp.GetRequiredService<RefineAnswerStep>()
            };
            return new QaSequentialWorkflow(steps, logger);
        });
        #endregion

        #region Concurrent
        // Concurrent Review Workflow Steps
        // Experts — inject expertId only
        services.AddTransient<ExpertExecutionStep>(sp =>
        {
            var agentFactory = sp.GetRequiredService<IAgentFactory>();
            var promptRepo = sp.GetRequiredService<IExpertPromptRepository>();
            var logger = sp.GetRequiredService<ILogger<ExpertExecutionStep>>();

            return new ExpertExecutionStep(agentFactory, promptRepo, logger, "CloudPrintFlowExpert");
        });

        services.AddTransient<ExpertExecutionStep>(sp =>
        {
            var agentFactory = sp.GetRequiredService<IAgentFactory>();
            var promptRepo = sp.GetRequiredService<IExpertPromptRepository>();
            var logger = sp.GetRequiredService<ILogger<ExpertExecutionStep>>();

            return new ExpertExecutionStep(agentFactory, promptRepo, logger, "IbonDocFormatExpert");
        });

        services.AddTransient<ExpertExecutionStep>(sp =>
        {
            var agentFactory = sp.GetRequiredService<IAgentFactory>();
            var promptRepo = sp.GetRequiredService<IExpertPromptRepository>();
            var logger = sp.GetRequiredService<ILogger<ExpertExecutionStep>>();

            return new ExpertExecutionStep(agentFactory, promptRepo, logger, "AiImageServiceExpert");
        });

        services.AddTransient<ExpertExecutionStep>(sp =>
        {
            var agentFactory = sp.GetRequiredService<IAgentFactory>();
            var promptRepo = sp.GetRequiredService<IExpertPromptRepository>();
            var logger = sp.GetRequiredService<ILogger<ExpertExecutionStep>>();

            return new ExpertExecutionStep(agentFactory, promptRepo, logger, "FlowKioskOperationExpert");
        });

        services.AddTransient<ExpertExecutionStep>(sp =>
        {
            var agentFactory = sp.GetRequiredService<IAgentFactory>();
            var promptRepo = sp.GetRequiredService<IExpertPromptRepository>();
            var logger = sp.GetRequiredService<ILogger<ExpertExecutionStep>>();

            return new ExpertExecutionStep(agentFactory, promptRepo, logger, "UXToneEmpathySpecialist");
        });

        services.AddTransient<ExpertExecutionStep>(sp =>
        {
            var agentFactory = sp.GetRequiredService<IAgentFactory>();
            var promptRepo = sp.GetRequiredService<IExpertPromptRepository>();
            var logger = sp.GetRequiredService<ILogger<ExpertExecutionStep>>();

            return new ExpertExecutionStep(agentFactory, promptRepo, logger, "FallbackExpert");
        });



        // Expert Prompt Repository（一次載入所有 md）
        services.AddSingleton<IExpertPromptRepository, ExpertPromptRepository>();

        #endregion

        #region Handoffs 

        // Handoffs Workflow Steps
        services.AddTransient<RoutingStep>();
        services.AddTransient<HandoffStep>();
        services.AddTransient<HandoffsMergeStep>();

        // Handoffs Workflow
        services.AddTransient<ISequentialWorkflow<HandoffsContext>, HandoffsWorkflow>();

        #endregion

        #region Groupchat

        // Groupchat Workflow
        services.AddTransient<ISequentialWorkflow<GroupchatContext>, GroupchatWorkflow>();
          
        #endregion

        // Merge Step
        services.AddTransient<ReviewMergeStep>();

        // The combined workflow
        services.AddTransient<ISequentialWorkflow<ConcurrentReviewContext>, ConcurrentReviewWorkflow>();



        // App Modes
        services.AddTransient<IAppMode, SimpleChatMode>();
        services.AddTransient<IAppMode, QaSequentialWorkflowMode>();
        services.AddTransient<IAppMode, ConcurrentReviewWorkflowMode>();
        services.AddTransient<IAppMode, HandoffsWorkflowMode>();
        services.AddTransient<IAppMode, GroupchatWorkflowMode>();

        // Workflow Factory
        services.AddSingleton<IWorkflowFactory, WorkflowFactory>();

        return services;
    }

    public static IServiceCollection AddCoreExecutionServices(this IServiceCollection services)
    {
        // Simple Chat
        services.AddTransient<SimpleChatExecutionService>();

        // 模式 2：Sequential QA
        services.AddTransient<QaSequentialExecutionService>();

        // 模式 3：Concurrent Review
        services.AddTransient<ConcurrentReviewExecutionService>();

        // 模式 4：Handoffs
        services.AddTransient<HandoffsExecutionService>();

        // 模式 5：Groupchat
        services.AddTransient<GroupchatExecutionService>();

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // 這裡放「跟 Workflow / Prompt / Runner 有關」的註冊，
        // 基本上就是你原本在 Console Program.cs 裡有的那些東西，搬過來這裡。

        // ✅ Prompt 儲存庫（讀 .md 專家檔的那個）
        services.AddSingleton<IExpertPromptRepository, ExpertPromptRepository>();

        // ✅ Workflow Runner
        services.AddTransient<IWorkflowRunner, WorkflowRunner>();

        services.AddTransient<RoutingStep>();
        services.AddTransient<HandoffsToolCallingStep>();
        services.AddTransient<HandoffStep>();
        services.AddTransient<HandoffsMergeStep>();

        // ✅ 各種 Workflow，本來在 Console 會註冊的那幾個：
        services.AddTransient<ISequentialWorkflow<QaSequentialContext>, QaSequentialWorkflow>();
        services.AddTransient<ISequentialWorkflow<ConcurrentReviewContext>, ConcurrentReviewWorkflow>();
        services.AddTransient<ISequentialWorkflow<HandoffsContext>, HandoffsWorkflow>();
        services.AddTransient<ISequentialWorkflow<GroupchatContext>, GroupchatWorkflow>();
         
        services.AddTransient<IWorkflowStep<QaSequentialContext>, RewriteQuestionStep>();
        services.AddTransient<IWorkflowStep<QaSequentialContext>, AnswerQuestionStep>();
        services.AddTransient<IWorkflowStep<QaSequentialContext>, RefineAnswerStep>();


        // （如果你有別的 Workflow，比如 SimpleChat 用不到 Workflow，就可以不用註冊）

        // MCP Options
        services.Configure<McpOptions>(configuration.GetSection("Mcp"));

        // MCP Http Caller
        services.AddHttpClient<HttpMcpToolCaller>();
        services.AddTransient<IMcpToolCaller, HttpMcpToolCaller>();

        return services;
    }

}

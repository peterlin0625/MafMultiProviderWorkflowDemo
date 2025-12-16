using System.Text.Json;
using MafDemo.Core.Mcp;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Workflows.Handoffs;

public sealed class HandoffsToolCallingStep : IWorkflowStep<HandoffsContext>
{
    private readonly IMcpToolCaller _mcpCaller;
    private readonly ILogger<HandoffsToolCallingStep> _logger;

    public HandoffsToolCallingStep(
        IMcpToolCaller mcpCaller,
        ILogger<HandoffsToolCallingStep> logger)
    {
        _mcpCaller = mcpCaller;
        _logger = logger;
    }

    public string Name => "HandoffsToolCallingStep";

    public async Task ExecuteAsync(HandoffsContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.TargetExpertId))
        {
            _logger.LogInformation("HandoffsToolCallingStep: 尚未決定 TargetExpertId，略過 Tool 呼叫。");
            return;
        }

        // 這裡示範：只有特定 Expert 才會呼叫 Tool A
        // 例如：CloudPrintProductExpert → 查詢商品樹（Tool A）
        if (string.Equals(context.TargetExpertId, "CloudPrintProductExpert", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("HandoffsToolCallingStep: TargetExpertId = {ExpertId}，呼叫 Tool A：CloudPrintProductSearch。",
                context.TargetExpertId);

            var args = new
            {
                question = context.OriginalQuestion,
                // 未來可以加更多欄位：userId, language, category...
            };

            var argsJson = JsonSerializer.Serialize(args);

            var resultJson = await _mcpCaller.CallToolAsync(
                toolName: "ToolA.CloudPrintProductSearch",
                argumentsJson: argsJson,
                cancellationToken: cancellationToken);

            context.ToolResultJson = resultJson;
        }
        else
        {
            _logger.LogInformation("HandoffsToolCallingStep: TargetExpertId = {ExpertId}，目前未設定 Tool 呼叫，略過。",
                context.TargetExpertId);
        }
    }
}

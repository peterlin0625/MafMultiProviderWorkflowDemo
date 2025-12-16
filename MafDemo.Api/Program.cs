using MafDemo.Api.Contracts;
using MafDemo.Core;
using MafDemo.Core.Execution;
using MafDemo.Core.Llm;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// =====================
//  1) Serilog 設定
// =====================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// =====================
//  2) DI 註冊
// =====================
builder.Services.AddLlmProviders(builder.Configuration);
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddCoreExecutionServices();

// =====================
//  3) Swagger 註冊
// =====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // XML Doc 支援
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.SwaggerDoc("v1", new()
    {
        Title = "MafDemo AI Workflow API",
        Version = "v1",
        Description = "提供 Simple / Sequential / Concurrent / Handoffs / Groupchat 五大 Workflow 執行"
    });
});

var app = builder.Build();

// =====================
//  4) Serilog Request Logging
// =====================
app.UseSerilogRequestLogging();

// =====================
//  5) 啟用 Swagger（最重要的兩行）
// =====================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MafDemo AI Workflow API v1");
    c.DocumentTitle = "MafDemo API";
});

// =====================
//  6) 測試 Endpoint
// =====================
app.MapGet("/", () => "MafDemo API Running.")
    .WithTags("System")
    .WithSummary("API 健康檢查");

// =====================
//  7) 核心 Workflow Endpoint
// =====================
app.MapPost("/api/workflow/{mode}", async (
    string mode,
    RunWorkflowRequest request,
    SimpleChatExecutionService simpleChat,
    QaSequentialExecutionService qaSequential,
    ConcurrentReviewExecutionService concurrentReview,
    HandoffsExecutionService handoffs,
    GroupchatExecutionService groupchat,
    IOptions<LlmOptions> llmOptions,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new { error = "Question 不可為空" });
    }

    var provider = llmOptions.Value.DefaultProvider;

    switch (mode.Trim().ToLowerInvariant())
    {
        case "1":
        case "simple":
            return Results.Ok(new
            {
                mode = "simple",
                provider,
                result = await simpleChat.RunAsync(request.Question, cancellationToken)
            });

        case "2":
        case "qa":
        case "sequential":
            return Results.Ok(new
            {
                mode = "sequential",
                provider,
                result = await qaSequential.RunAsync(request.Question, cancellationToken)
            });

        case "3":
        case "concurrent":
            return Results.Ok(new
            {
                mode = "concurrent",
                provider,
                result = await concurrentReview.RunAsync(request.Question, cancellationToken)
            });

        case "4":
        case "handoffs":
            return Results.Ok(new
            {
                mode = "handoffs",
                provider,
                result = await handoffs.RunAsync(request.Question, cancellationToken)
            });

        case "5":
        case "groupchat":
            return Results.Ok(new
            {
                mode = "groupchat",
                provider,
                result = await groupchat.RunAsync(request.Question, cancellationToken)
            });

        default:
            return Results.BadRequest(new
            {
                error = $"Unknown mode: {mode}"
            });
    }
})
.WithTags("Workflow")
.WithSummary("執行指定 Workflow 模式")
.WithDescription("支援 Simple / Sequential / Concurrent / Handoffs / Groupchat 模式。");

app.Run();

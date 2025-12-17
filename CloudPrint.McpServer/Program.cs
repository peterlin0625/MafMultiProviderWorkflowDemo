using CloudPrint.McpServer.DataApi;
using MafDemo.McpServer.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using CloudPrint.McpServer.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModelContextProtocol.AspNetCore;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);



// 註冊 Style 相關服務（包含 StyleApiClient）
builder.Services.AddStyleServices(builder.Configuration);

// 註冊所有 MCP Tools
builder.Services
    .AddMcpServer()
    .WithToolsFromAssembly()
    .WithHttpTransport();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7282, listenOptions =>
    {
        listenOptions.UseHttps(); // 開發環境會使用 dev cert；生產請指定 PFX
    });
});

var app = builder.Build();

// MCP Endpoint: /mcp
app.MapMcp("/mcp");


 // windows cmd
 //curl - k - X POST "https://localhost:7282/mcp/tools/call" ^
 // -H "Content-Type: application/json" ^
 // -d "{\"tool\":\"getServerTime\",\"arguments\":{}}"

// 兼容性：提供一個簡單的 HTTP proxy，接受 POST /mcp/tools/call
// Payload: { "tool": "toolName", "arguments": { ... } }
app.MapPost("/mcp/tools/call", async (HttpContext http) =>
{
    // 防守式解析：先將 body 緩衝到記憶體，回傳 400 當 JSON 不合法
    http.Request.EnableBuffering();
    string bodyText;
    using (var sr = new StreamReader(http.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
    {
        bodyText = await sr.ReadToEndAsync().ConfigureAwait(false);
        http.Request.Body.Position = 0;
    }

    JsonDocument doc;
    try
    {
        doc = JsonDocument.Parse(bodyText);
    }
    catch (JsonException)
    { 
        return Results.BadRequest(new { error = "Invalid JSON payload" });
    }

    using (doc)
    {
        var root = doc.RootElement;

        if (!root.TryGetProperty("tool", out var toolEl) || toolEl.ValueKind != JsonValueKind.String)
            return Results.BadRequest(new { error = "Missing or invalid 'tool' property" });

        var toolName = toolEl.GetString()!;
        var argsEl = root.TryGetProperty("arguments", out var a) ? a : default(JsonElement);

        // 掃描當前 Assembly 的 MCP tools（避免直接引用 attribute 型別，改用名稱比對）
        var asm = Assembly.GetExecutingAssembly();
        var toolMethods = asm.GetTypes()
            .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == "McpServerToolTypeAttribute"))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes().Any(a => a.GetType().Name == "McpServerToolAttribute")))
            .ToList();

        MethodInfo? target = null;
        foreach (var m in toolMethods)
        {
            var attrObj = m.GetCustomAttributes().First(a => a.GetType().Name == "McpServerToolAttribute");
            var nameProp = attrObj.GetType().GetProperty("Name");
            var name = (nameProp?.GetValue(attrObj) as string) ?? m.Name;
            if (string.Equals(name, toolName, StringComparison.OrdinalIgnoreCase) || string.Equals(m.Name, toolName, StringComparison.OrdinalIgnoreCase))
            {
                target = m;
                break;
            }
        }

        if (target is null)
            return Results.NotFound(new { error = "Tool not found" });

        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;

        object? instance = null;
        if (!target.IsStatic)
        {
            try
            {
                instance = sp.GetService(target.DeclaringType!) ?? ActivatorUtilities.CreateInstance(sp, target.DeclaringType!);
            }
            catch(Exception ex) {
                return Results.Problem(detail: $"Failed to create instance of '{target.DeclaringType?.FullName}': {ex.Message}");
            }
            
        }

        var parameters = target.GetParameters();
        object?[] invokeArgs;

        if (parameters.Length == 0)
        {
            invokeArgs = Array.Empty<object?>();
        }
        else if (parameters.Length == 1)
        {
            var pType = parameters[0].ParameterType;
            if (argsEl.ValueKind == JsonValueKind.Undefined || argsEl.ValueKind == JsonValueKind.Null)
            {
                invokeArgs = new object?[] { null };
            }
            else
            {
                var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var arg = JsonSerializer.Deserialize(argsEl.GetRawText(), pType, opt);
                invokeArgs = new object?[] { arg };
            }
        }
        else
        {
            // 多參數：嘗試用 arguments 物件對應每個參數名稱
            invokeArgs = new object?[parameters.Length];
            if (argsEl.ValueKind == JsonValueKind.Object)
            {
                var opt = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                for (int i = 0; i < parameters.Length; i++)
                {
                    var p = parameters[i];
                    if (argsEl.TryGetProperty(p.Name!, out var prop))
                    {
                        invokeArgs[i] = JsonSerializer.Deserialize(prop.GetRawText(), p.ParameterType, opt);
                    }
                    else
                    {
                        invokeArgs[i] = p.HasDefaultValue ? p.DefaultValue : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null);
                    }
                }
            }
            else
            {
                // 無法映射
                return Results.BadRequest(new { error = "arguments must be an object when method has multiple parameters" });
            }
        }

        object? invokeResult;
        try
        {
            invokeResult = target.Invoke(instance, invokeArgs);
        }
        catch (TargetInvocationException tie)
        {
            return Results.Problem(detail: tie.InnerException?.Message ?? tie.Message);
        }

        if (invokeResult is Task task)
        {
            await task.ConfigureAwait(false);
            var tType = task.GetType();
            if (tType.IsGenericType)
            {
                var resultProp = tType.GetProperty("Result");
                var value = resultProp?.GetValue(task);
                return Results.Json(value);
            }
            else
            {
                return Results.Json(null);
            }
        }

        return Results.Json(invokeResult);
    }
});

// curl -k https://localhost:7282/mcp/tools/list
// 列出可用 tools，方便用瀏覽器檢視
app.MapGet("/mcp/tools/list", () =>
{
    var asm = Assembly.GetExecutingAssembly();
    var tools = asm.GetTypes()
        .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == "McpServerToolTypeAttribute"))
        .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.GetCustomAttributes().Any(a => a.GetType().Name == "McpServerToolAttribute"))
            .Select(m =>
            {
                var attrObj = m.GetCustomAttributes().First(a => a.GetType().Name == "McpServerToolAttribute");
                var nameProp = attrObj.GetType().GetProperty("Name");
                var name = (nameProp?.GetValue(attrObj) as string) ?? m.Name;
                return new
                {
                    Name = name,
                    Method = m.Name,
                    DeclaringType = m.DeclaringType?.FullName,
                    Parameters = m.GetParameters().Select(p => new { p.Name, Type = p.ParameterType.Name })
                };
            }))
        .ToList();

    return Results.Json(tools);
});

app.Run();

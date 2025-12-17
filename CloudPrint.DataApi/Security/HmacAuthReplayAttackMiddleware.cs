using CommonLibrary.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CloudPrint.DataApi.Security;

public class HmacAuthReplayAttackMiddleware : IMiddleware
{
     
    private readonly HmacAuthOptions _options;
    private readonly ILogger<HmacAuthReplayAttackMiddleware> _logger;

    public HmacAuthReplayAttackMiddleware(
        IOptions<HmacAuthOptions> options, 
        ILogger<HmacAuthReplayAttackMiddleware> logger)
    {
        _options = options.Value; 
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // 排除 HealthCheck 或 swagger
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // 取得 headers
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey) ||
            !context.Request.Headers.TryGetValue("X-Timestamp", out var ts) ||
            !context.Request.Headers.TryGetValue("X-Signature", out var sig))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing HMAC headers");
            return;
        }

        if (apiKey != _options.ApiKey)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Invalid Api Key");
            return;
        }

        // 檢查時間戳避免重放攻擊
        if (!long.TryParse(ts, out long unixTs))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Timestamp");
            return;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(unixTs);
        if (DateTimeOffset.UtcNow - requestTime > TimeSpan.FromMinutes(5))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Timestamp too old");
            return;
        }

        // 計算 Body Hash
        context.Request.EnableBuffering();
        string body = "";
        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        //var bodyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));

        // 計算簽章
        //var signingString = $"{ts}:{context.Request.Method}:{context.Request.Path}:{bodyHash}";
        //var keyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
        //var hmac = new HMACSHA256(keyBytes);
        //var computedSig = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signingString)));

        // 計算簽章
        var computedSigLower = HmacSignature.CreateSignature(
            _options.SecretKey,
            ts.ToString(),
            context.Request.Method,
            context.Request.Path,
            body);

        if (!computedSigLower.Equals(sig, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Signature");
            return;
        }

        await next(context);
    }
}

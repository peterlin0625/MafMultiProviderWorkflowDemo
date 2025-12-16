using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace CloudPrint.DataApi.Security;

public class HmacAuthMiddleware : IMiddleware
{
    private const int CLOCK_SKEW_SECONDS = 120;
    private const int NONCE_BUFFER_SECONDS = 60; // buffer

    private readonly HmacAuthOptions _options;
    private readonly ILogger<HmacAuthMiddleware> _logger;
    private readonly IMemoryCache _cache;

    public HmacAuthMiddleware(
        IOptions<HmacAuthOptions> options,
        IMemoryCache cache,
        ILogger<HmacAuthMiddleware> logger)
    {
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // 1️⃣ 排除路徑
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // 2️⃣ Headers
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

        // 3️⃣ Clock Skew ±120s
        if (!long.TryParse(ts, out var unixTs))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Timestamp");
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - unixTs) > CLOCK_SKEW_SECONDS)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Timestamp out of range");
            return;
        }

        // 4️⃣ Nonce（只對非 GET）
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            if (!context.Request.Headers.TryGetValue("X-Nonce", out var nonce))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Missing Nonce");
                return;
            }

            var nonceKey = $"hmac:nonce:{apiKey}:{nonce}";

            if (_cache.TryGetValue(nonceKey, out _))
            {
                context.Response.StatusCode = 409; // Conflict
                await context.Response.WriteAsync("Replay detected");
                return;
            }

            _cache.Set(
                nonceKey,
                true,
                TimeSpan.FromSeconds(CLOCK_SKEW_SECONDS + NONCE_BUFFER_SECONDS)
            );
        }

        // 5️⃣ Body Hash
        context.Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        var bodyHash =
            Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(body))
            ).ToLowerInvariant();

        // 6️⃣ Signature
        var signingString =
            $"{ts}:{context.Request.Method}:{context.Request.Path}:{bodyHash}";

        using var hmac =
            new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));

        var computedSig =
            Convert.ToHexString(
                hmac.ComputeHash(Encoding.UTF8.GetBytes(signingString))
            ).ToLowerInvariant();

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSig),
                Encoding.UTF8.GetBytes(sig.ToString().ToLowerInvariant())))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Signature");
            return;
        }

        await next(context);
    }
}

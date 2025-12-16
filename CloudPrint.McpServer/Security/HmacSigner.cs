using System.Security.Cryptography;
using System.Text;

namespace MafDemo.McpServer.Security;

public sealed class HmacSigner
{
    private readonly string _apiKey;
    private readonly byte[] _secretBytes;

    public HmacSigner(string apiKey, string apiSecret)
    {
        _apiKey = apiKey;
        _secretBytes = Encoding.UTF8.GetBytes(apiSecret);
    }

    public HmacSignature CreateSignature(
        string httpMethod,
        string path,
        string body = "")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var bodyHash = ComputeSha256(body);

        // ⚠ 完全依照你指定的格式
        var signingString = $"{timestamp}:{httpMethod}:{path}:{bodyHash}";

        using var hmac = new HMACSHA256(_secretBytes);
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingString));
        var signature = Convert.ToHexString(signatureBytes).ToLowerInvariant();

        return new HmacSignature
        {
            ApiKey = _apiKey,
            Timestamp = timestamp,
            Signature = signature
        };
    }

    private static string ComputeSha256(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));  
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public sealed class HmacSignature
{
    public string ApiKey { get; init; } = "";
    public string Timestamp { get; init; } = "";
    public string Signature { get; init; } = "";
}

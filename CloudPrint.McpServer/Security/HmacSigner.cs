using System;
using System.Security.Cryptography;
using System.Text;

using lib = CommonLibrary.Security;

namespace MafDemo.McpServer.Security;

public sealed class HmacSigner
{
    private readonly string _apiKey;
    private readonly string _secretKey;

    public HmacSigner(string apiKey, string apiSecret)
    {
        _apiKey = apiKey;
        _secretKey = apiSecret;
    }

    public HmacSignature CreateSignature(
        string httpMethod,
        string path,
        string body = "")
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
 
        // 計算簽章
        var computedSigLower = lib.HmacSignature.CreateSignature(
            _secretKey,
            timestamp,
            httpMethod,
            path,
            body);

        return new HmacSignature
        {
            ApiKey = _apiKey,
            Timestamp = timestamp,
            Signature = computedSigLower
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

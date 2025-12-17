using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CommonLibrary.Security
{
    public static class HmacSignature
    {

        public static string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        public static string CreateSignature(
            string SecretKey,
            string timestamp,
            string httpMethod,
            string path,
            string body = "")
        {
            // 計算 Body Hash
            var bodyHash = ComputeSha256(body);
            // 自定義的格式
            var signingString = $"{timestamp}:{httpMethod}:{path}:{bodyHash}";
            // 計算簽章
            var signature = ComputeHmacSha256(SecretKey, signingString);

            return signature;
        }
        public static string ComputeHmacSha1(string key, string message)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA1(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        public static string ComputeHmacSha256(string key, string message)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        public static string ComputeSha256(string input)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}

using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Json;

/// <summary>
/// 專門用來處理 LLM 產生的「接近 JSON 但不完全合法」的文字。
/// </summary>
public static class JsonSanitizer
{
    /// <summary>
    /// 嘗試從原始文字中抽出並修正為可被 JsonDocument.Parse 的 JSON 物件字串。
    /// 會：
    /// 1. 去掉 ```json 這類 markdown 包裝
    /// 2. 抓第一個 '{' 到最後一個 '}'
    /// 3. 用大括號數量做簡單的「補 / 修」動作
    /// 4. 測試 Parse，成功才回傳 true
    /// </summary>
    public static bool TryExtractValidJson(string raw, out string json, ILogger? logger = null)
    {
        json = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            logger?.LogWarning("JsonSanitizer: 原始內容為空，無法抽取 JSON。");
            return false;
        }

        var cleaned = StripMarkdownFences(raw);
        logger?.LogInformation("JsonSanitizer: 清理後原始內容 = {Cleaned}", cleaned);

        var first = cleaned.IndexOf('{');
        var last = cleaned.LastIndexOf('}');

        if (first < 0 || last <= first)
        {
            logger?.LogWarning("JsonSanitizer: 無法找到 '{' 或 '}'，放棄抽取。");
            return false;
        }

        var segment = cleaned.Substring(first, last - first + 1);

        // 先做一次「大括號平衡修正」
        var balanced = BalanceCurlyBraces(segment, logger);

        // 優先嘗試 balanced
        if (TryParse(balanced, logger, label: "balanced"))
        {
            json = balanced;
            return true;
        }

        // balanced 還是不行，就退回原 segment 再試一次
        if (TryParse(segment, logger, label: "segment"))
        {
            json = segment;
            return true;
        }

        logger?.LogWarning("JsonSanitizer: balanced / segment 都無法解析為 JSON。");
        return false;
    }

    private static string StripMarkdownFences(string raw)
    {
        var s = raw.Trim();

        // 去掉 ```json / ``` 這類程式碼區塊
        if (s.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewLine = s.IndexOf('\n');
            if (firstNewLine >= 0)
            {
                s = s[(firstNewLine + 1)..];
            }
        }

        if (s.EndsWith("```", StringComparison.Ordinal))
        {
            var lastFence = s.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence >= 0)
            {
                s = s[..lastFence];
            }
        }

        return s.Trim();
    }

    /// <summary>
    /// 用最簡單的大括號數量檢查來補 / 削多餘的大括號。
    /// 不保證 100% 正確，但對 LLM 輸出的小錯誤通常有效。
    /// </summary>
    private static string BalanceCurlyBraces(string input, ILogger? logger)
    {
        int open = 0;
        int close = 0;

        foreach (var ch in input)
        {
            if (ch == '{') open++;
            else if (ch == '}') close++;
        }

        logger?.LogInformation("JsonSanitizer: BalanceCurlyBraces 前，大括號統計 open={Open}, close={Close}", open, close);

        var sb = new StringBuilder(input);

        if (open > close)
        {
            // 缺少的 '}' 數量
            var missing = open - close;
            logger?.LogWarning("JsonSanitizer: 缺少 {Missing} 個 '}}'，自動在字串尾端補上。", missing);
            for (int i = 0; i < missing; i++)
            {
                sb.Append('}');
            }
        }
        else if (close > open)
        {
            // '}' 過多，從尾巴開始刪到數量相等
            var extra = close - open;
            logger?.LogWarning("JsonSanitizer: 多出 {Extra} 個 '}}'，自動從字串尾端刪除。", extra);

            for (int i = sb.Length - 1; i >= 0 && extra > 0; i--)
            {
                if (sb[i] == '}')
                {
                    sb.Remove(i, 1);
                    extra--;
                }
            }
        }

        var result = sb.ToString();
        logger?.LogInformation("JsonSanitizer: BalanceCurlyBraces 後 = {Result}", result);
        return result;
    }

    private static bool TryParse(string json, ILogger? logger, string label)
    {
        try
        {
            using var _ = JsonDocument.Parse(json);
            logger?.LogInformation("JsonSanitizer: {Label} JSON 解析成功。", label);
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "JsonSanitizer: {Label} JSON 解析失敗。內容 = {Json}", label, json);
            return false;
        }
    }
}

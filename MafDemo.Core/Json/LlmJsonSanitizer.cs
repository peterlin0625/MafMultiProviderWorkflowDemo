using System;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Json;

/// <summary>
/// 專門處理 LLM 輸出 JSON 的「抽取 + 修補」工具。
/// 目標：
/// 1. 去掉 ```json 這類程式碼區塊標記
/// 2. 抽出最有可能是 JSON 的片段
/// 3. 修補簡單的括號不平衡（少一個 ] 或 }）
/// </summary>
public sealed class LlmJsonSanitizer
{
    private readonly ILogger<LlmJsonSanitizer> _logger;

    public LlmJsonSanitizer(ILogger<LlmJsonSanitizer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 給外部用的主入口：從 LLM 原始輸出，拿到「盡可能可 parse 的 JSON 字串」。
    /// </summary>
    public string Sanitize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            _logger.LogWarning("LlmJsonSanitizer.Sanitize 收到空字串。");
            return string.Empty;
        }

        _logger.LogInformation("LlmJsonSanitizer.Sanitize 原始內容 = {Raw}", raw);

        var text = RemoveCodeFences(raw).Trim();

        // 如果本來就是從 { 開始到 } 結束，先試一次
        if (text.StartsWith("{") && text.EndsWith("}"))
        {
            var fixedJson = FixBracketImbalance(text);
            _logger.LogInformation("LlmJsonSanitizer: 採用整段 JSON（長度 {Len}）。", fixedJson.Length);
            return fixedJson;
        }

        // 否則試著從中間抽一段「看起來像 JSON」的片段
        var fragment = ExtractBalancedJsonFragment(text);
        var fixedFragment = FixBracketImbalance(fragment);

        _logger.LogInformation("LlmJsonSanitizer: 最終輸出 JSON 片段 = {Json}", fixedFragment);
        return fixedFragment;
    }

    /// <summary>
    /// 移除 ```json ... ``` 類型的 Markdown code fences。
    /// </summary>
    private string RemoveCodeFences(string raw)
    {
        // 最簡單暴力版：移除 ```json / ``` / ```JSON
        var cleaned = Regex.Replace(raw, "```json", string.Empty, RegexOptions.IgnoreCase);
        cleaned = cleaned.Replace("```", string.Empty);
        return cleaned;
    }

    /// <summary>
    /// 嘗試從文字中抽出一段「括號平衡」的 JSON 片段。
    /// </summary>
    private string ExtractBalancedJsonFragment(string text)
    {
        var first = text.IndexOf('{');
        if (first < 0)
        {
            _logger.LogWarning("LlmJsonSanitizer: 找不到 '{'，直接回傳原始文字。");
            return text;
        }

        // 嘗試從第一個 '{' 開始，往後找「大括號平衡歸零」的位置
        int depth = 0;
        int lastIndex = -1;
        for (int i = first; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '{') depth++;
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    lastIndex = i;
                    break;
                }
            }
        }

        if (lastIndex > first)
        {
            var fragment = text.Substring(first, lastIndex - first + 1);
            _logger.LogInformation("LlmJsonSanitizer: ExtractBalancedJsonFragment 抽出 = {Fragment}", fragment);
            return fragment;
        }

        // 若無法平衡，就退而求其次：取第一個 '{' 到最後一個 '}'，再交給 FixBracketImbalance 處理
        var last = text.LastIndexOf('}');
        if (last > first)
        {
            var fragment = text.Substring(first, last - first + 1);
            _logger.LogWarning("LlmJsonSanitizer: 無法完全平衡，只好取 first..last 片段 = {Fragment}", fragment);
            return fragment;
        }

        _logger.LogWarning("LlmJsonSanitizer: 無法找到 '}}'，回傳從 first 到結尾的片段。");
        return text.Substring(first);
    }

    /// <summary>
    /// 修補簡單的中/大括號不平衡（例如少一個 ] 或 }）。
    /// 不保證完全 correct JSON，只求 JsonDocument.Parse 比較有機會成功。
    /// </summary>
    private string FixBracketImbalance(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        var sb = new StringBuilder(json);

        int openCurly = 0, closeCurly = 0;
        int openSquare = 0, closeSquare = 0;

        foreach (var ch in json)
        {
            switch (ch)
            {
                case '{': openCurly++; break;
                case '}': closeCurly++; break;
                case '[': openSquare++; break;
                case ']': closeSquare++; break;
            }
        }

        if (openSquare > closeSquare)
        {
            int need = openSquare - closeSquare;
            _logger.LogWarning("LlmJsonSanitizer: 偵測到缺少 {Count} 個 ']'，嘗試補在結尾。", need);
            sb.Append(']', need);
        }

        if (openCurly > closeCurly)
        {
            int need = openCurly - closeCurly;
            _logger.LogWarning("LlmJsonSanitizer: 偵測到缺少 {Count} 個 '}}'，嘗試補在結尾。", need);
            sb.Append('}', need);
        }

        return sb.ToString();
    }
}

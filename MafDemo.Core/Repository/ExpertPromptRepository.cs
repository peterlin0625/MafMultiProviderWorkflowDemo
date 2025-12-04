using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MafDemo.Core.Repository;

public interface IExpertPromptRepository
{
    string GetPromptFor(string expertId);
}

public sealed class ExpertPromptRepository : IExpertPromptRepository
{
    private readonly Dictionary<string, string> _cache = new();
    private readonly string _root;
    private readonly ILogger<ExpertPromptRepository> _logger;

    public ExpertPromptRepository(
        PromptOptions promptOptions,
        IHostEnvironment env,
        ILogger<ExpertPromptRepository> logger)
    {
        _logger = logger;
        _root = Path.Combine(env.ContentRootPath, promptOptions.ExpertsPromptDirectory);

        if (!Directory.Exists(_root))
        {
            logger.LogWarning("Experts prompt directory not found: {Root}", _root);
        }

        // 預先把所有 .md 全讀起來（只讀一次）
        if (Directory.Exists(_root))
        {
            foreach (var file in Directory.GetFiles(_root, "*.md"))
            {
                var key = Path.GetFileNameWithoutExtension(file); // e.g. WritingExpert
                _cache[key] = File.ReadAllText(file);
            }
        }
    }

    public string GetPromptFor(string expertId)
    {
        if (_cache.TryGetValue(expertId, out var content))
            return content;

        _logger.LogWarning("Expert prompt not found for ID: {Id}", expertId);
        return $"你是一個名為 {expertId} 的專家，請提供專業建議。";
    }
}

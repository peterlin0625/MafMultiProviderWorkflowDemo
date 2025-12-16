using MafDemo.Core.Modes;

namespace MafDemo.Core.Workflows;

public sealed class WorkflowFactory : IWorkflowFactory
{
    private readonly Dictionary<string, IAppMode> _byId;
    private readonly Dictionary<string, IAppMode> _byName;

    public WorkflowFactory(IEnumerable<IAppMode> modes)
    {
        // Id: "1", "2", "3", "4", "5"...
        _byId = modes
            .GroupBy(m => m.Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // DisplayName: "模式 1：...", "模式 2：..."
        _byName = modes
            .GroupBy(m => m.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IAppMode GetMode(string idOrName)
    {
        if (string.IsNullOrWhiteSpace(idOrName))
            throw new ArgumentNullException(nameof(idOrName));

        if (_byId.TryGetValue(idOrName, out var byId))
            return byId;

        if (_byName.TryGetValue(idOrName, out var byName))
            return byName;

        throw new KeyNotFoundException($"找不到模式：{idOrName}");
    }

    public IReadOnlyCollection<IAppMode> GetAllModes()
        => _byId.Values
            .OrderBy(m => m.Id)
            .ToArray();
}

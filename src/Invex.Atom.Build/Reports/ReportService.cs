namespace Invex.Atom.Build.Reports;

/// <summary>
///     The default thread-safe <see cref="IReportService" />.
/// </summary>
internal sealed class ReportService : IReportService
{
    private readonly Lock _lock = new();
    private readonly List<string> _targetOrder = [];
    private readonly Dictionary<string, List<IReportData>> _reportData = [];

    /// <inheritdoc />
    public void AddReportData(IReportData reportData, string? targetName = null)
    {
        targetName ??= string.Empty;

        lock (_lock)
        {
            if (!_reportData.TryGetValue(targetName, out var targetData))
            {
                _reportData[targetName] = targetData = [];
                _targetOrder.Add(targetName);
            }

            targetData.Add(reportData);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<IReportData> GetReportData()
    {
        lock (_lock)
            return _targetOrder
                .SelectMany(x => _reportData[x])
                .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<IReportData> GetReportData(string? targetName)
    {
        lock (_lock)
            return _reportData.TryGetValue(targetName ?? string.Empty, out var targetData)
                ? targetData.ToArray()
                : [];
    }
}

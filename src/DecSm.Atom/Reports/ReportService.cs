namespace DecSm.Atom.Reports;

/// <summary>
///     A service for collecting and managing data to be included in build reports.
/// </summary>
/// <remarks>
///     This service acts as a central repository for all reportable information generated during a build.
///     Data is collected and can be retrieved for rendering by a report writer.
/// </remarks>
[PublicAPI]
public sealed class ReportService
{
    private readonly Dictionary<string, List<IReportData>> _reportData = [];

    /// <summary>
    ///     Adds a report data item, optionally associating it with a specific target.
    /// </summary>
    /// <param name="reportData">The report data to add.</param>
    /// <param name="targetName">The name of the target to associate the data with. If null, the data is stored globally.</param>
    public void AddReportData(IReportData reportData, string? targetName = null)
    {
        targetName ??= string.Empty;

        if (!_reportData.ContainsKey(targetName))
            _reportData[targetName] = [];

        _reportData[targetName]
            .Add(reportData);
    }

    /// <summary>
    ///     Retrieves all report data items collected during the build.
    /// </summary>
    /// <returns>A flattened list of all <see cref="IReportData" /> items from all targets.</returns>
    public List<IReportData> GetReportData() =>
        _reportData
            .SelectMany(x => x.Value)
            .ToList();
}

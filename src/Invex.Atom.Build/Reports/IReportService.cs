namespace Invex.Atom.Build.Reports;

/// <summary>
///     Collects data to be included in build reports.
/// </summary>
/// <remarks>
///     This service acts as a central repository for all reportable information generated during a build.
///     Data is collected and can be retrieved for rendering by an <see cref="IOutcomeReportWriter" />.
///     Implementations are safe to call from multiple threads.
/// </remarks>
[PublicAPI]
public interface IReportService
{
    /// <summary>
    ///     Adds a report data item, optionally associating it with a specific target.
    /// </summary>
    /// <param name="reportData">The report data to add.</param>
    /// <param name="targetName">
    ///     The name of the target to associate the data with, or <c>null</c> to store it without a target.
    /// </param>
    void AddReportData(IReportData reportData, string? targetName = null);

    /// <summary>
    ///     Retrieves all report data items collected during the build.
    /// </summary>
    /// <returns>
    ///     A snapshot of all <see cref="IReportData" /> items, grouped by target in the order each target first reported.
    /// </returns>
    IReadOnlyList<IReportData> GetReportData();

    /// <summary>
    ///     Retrieves the report data items associated with a specific target.
    /// </summary>
    /// <param name="targetName">The name of the target, or <c>null</c> for data stored without a target.</param>
    /// <returns>A snapshot of the <see cref="IReportData" /> items for the target, in the order they were added.</returns>
    IReadOnlyList<IReportData> GetReportData(string? targetName);
}

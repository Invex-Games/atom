namespace DecSm.Atom.Reports;

/// <summary>
///     Provides a helper for adding data to the build report.
/// </summary>
[PublicAPI]
public interface IReportsHelper : IBuildAccessor
{
    /// <summary>
    ///     Adds a data item to the build report.
    /// </summary>
    /// <remarks>
    ///     The collected data will be rendered by the appropriate report writer. For local builds, this is typically
    ///     the console. In a CI/CD environment, it may be included in the run summary.
    /// </remarks>
    /// <param name="reportData">The report data to add.</param>
    void AddReportData(IReportData reportData) =>
        GetService<ReportService>()
            .AddReportData(reportData);
}

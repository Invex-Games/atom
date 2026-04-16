namespace DecSm.Atom.Reports;

/// <summary>
///     Defines a contract for writing a build outcome report to a specific destination.
/// </summary>
/// <remarks>
///     Implementations of this interface are responsible for formatting and outputting the final build report.
///     Multiple writers can be registered to support different output targets, such as the console or CI/CD job summaries.
/// </remarks>
[PublicAPI]
public interface IOutcomeReportWriter
{
    /// <summary>
    ///     Generates and outputs a report of the build execution outcome.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous reporting operation.</returns>
    /// <remarks>
    ///     This method is called by the build executor after all targets have completed. Implementations should
    ///     access the necessary build state and report data through injected services and format it for their
    ///     specific output target.
    /// </remarks>
    Task ReportRunOutcome(CancellationToken cancellationToken);
}

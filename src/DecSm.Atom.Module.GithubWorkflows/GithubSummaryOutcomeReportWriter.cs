namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Implements <see cref="IOutcomeReportWriter" /> to write build outcome reports to GitHub Actions step summary.
/// </summary>
/// <remarks>
///     This writer formats the build report data into Markdown and appends it to the
///     `GITHUB_STEP_SUMMARY` file, making the report visible directly in the GitHub Actions UI.
///     It also masks any secrets present in the report text.
/// </remarks>
internal sealed class GithubSummaryOutcomeReportWriter(
    IAtomFileSystem fileSystem,
    ReportService reportService,
    IParamService paramService
) : IOutcomeReportWriter
{
    /// <summary>
    ///     Writes the build outcome report to the GitHub Actions step summary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        await using var writer = fileSystem.File.OpenWrite(Github.Variables.StepSummary);

        var reportText = ReportDataMarkdownFormatter.Write(reportService.GetReportData());

        // If the text contains any secrets, we don't want to log it
        reportText = paramService.MaskMatchingSecrets(reportText);

        await writer.WriteAsync(Encoding.UTF8.GetBytes(reportText), cancellationToken);
    }
}

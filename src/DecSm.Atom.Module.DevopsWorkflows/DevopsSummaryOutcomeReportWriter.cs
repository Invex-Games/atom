namespace DecSm.Atom.Module.DevopsWorkflows;

/// <summary>
///     Implements <see cref="IOutcomeReportWriter" /> to write build outcome reports to Azure DevOps build summary.
/// </summary>
/// <remarks>
///     This writer formats the build report data into Markdown, saves it to a temporary file,
///     and then uses an Azure DevOps logging command to upload it as a build summary.
///     It also masks any secrets present in the report text.
/// </remarks>
internal sealed class DevopsSummaryOutcomeReportWriter(
    IAtomFileSystem fileSystem,
    ReportService reportService,
    IParamService paramService
) : IOutcomeReportWriter
{
    /// <summary>
    ///     Writes the build outcome report to the Azure DevOps build summary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        var tempFile = fileSystem.AtomTempDirectory / "DevopsSummaryOutcome.md";

        if (tempFile.FileExists)
            fileSystem.File.Delete(tempFile);

        var reportText = ReportDataMarkdownFormatter.Write(reportService.GetReportData());

        // If the text contains any secrets, we don't want to log it
        reportText = paramService.MaskMatchingSecrets(reportText);

        var content = Encoding.UTF8.GetBytes(reportText);

        if (content.Length is 0)
            return;

        await using (var writer = fileSystem.File.Create(tempFile))
            await writer.WriteAsync(content, cancellationToken);

        // Azure DevOps logging command to upload a summary file
        Console.WriteLine($"##vso[task.uploadsummary]{tempFile}");
    }
}

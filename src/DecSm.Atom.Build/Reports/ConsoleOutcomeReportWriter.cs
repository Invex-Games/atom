namespace DecSm.Atom.Build.Reports;

/// <summary>
///     An implementation of <see cref="IOutcomeReportWriter" /> that writes a formatted build report to the console.
/// </summary>
/// <param name="args">The parsed command-line arguments.</param>
/// <param name="console">The console for writing output.</param>
/// <param name="buildModel">The model representing the build structure and state.</param>
/// <param name="reportService">The service containing the collected report data.</param>
/// <param name="paramService">The service for masking secrets.</param>
internal partial class ConsoleOutcomeReportWriter(
    CommandLineArgs args,
    IAnsiConsole console,
    BuildModel buildModel,
    ReportService reportService,
    IParamService paramService
) : IOutcomeReportWriter
{
    /// <summary>
    ///     Generates and writes a build summary and detailed report data to the console.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token (not used in this implementation).</param>
    public Task ReportRunOutcome(CancellationToken cancellationToken)
    {
        var table = new Table()
            .HideHeaders()
            .Border(TableBorder.Minimal)
            .AddColumn("Target", c => c.LeftAligned())
            .AddColumn("Outcome", c => c.LeftAligned())
            .AddColumn("Duration", c => c.LeftAligned());

        foreach (var state in buildModel
                     .TargetStates
                     .Select(x => x.Value)
                     .Where(x => x.Status is not TargetRunState.Skipped))
        {
            var outcome = state.Status switch
            {
                TargetRunState.Succeeded => "[green]Succeeded[/]",
                TargetRunState.Failed => "[red]Failed[/]",
                TargetRunState.NotRun or TargetRunState.PendingRun => "[yellow]NotRun[/]",
                var runState => $"[red]Unexpected state: {runState}[/]",
            };

            var targetDuration = state.RunDuration;

            var durationText = state.Status is TargetRunState.Succeeded or TargetRunState.Failed &&
                               targetDuration is not null
                ? targetDuration.Value.TotalMilliseconds < 10
                    ? "<0.01s"
                    : $"{targetDuration.Value.TotalSeconds:0.00}s"
                : string.Empty;

            if (state.Status is TargetRunState.NotRun && args.HasHeadless)
                continue;

            table.AddRow(paramService.MaskMatchingSecrets(state.Name), outcome, durationText);
        }

        console.WriteLine();
        console.Write(new Text("Build Summary", new(decoration: Decoration.Underline)));
        console.WriteLine();
        console.Write(table);
        console.WriteLine();

        if (!args.HasHeadless)
            Write(reportService.GetReportData());

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Formats a string for display by masking secrets and removing emojis.
    /// </summary>
    [return: NotNullIfNotNull("input")]
    private string? FormatFreetext(string? input) =>
        input is not null
            ? paramService.MaskMatchingSecrets(EmojiRegex()
                .Replace(input, string.Empty))
            : null;

    /// <summary>
    ///     Dispatches a list of report data items to the appropriate `Write` method based on their type.
    /// </summary>
    private void Write(IReadOnlyList<IReportData> reportData)
    {
        var customPreReportData = reportData
            .OfType<ICustomReportData>()
            .Where(x => x.BeforeStandardData)
            .ToList();

        var logReportData = reportData
            .OfType<LogReportData>()
            .ToList();

        var artifactReportData = reportData
            .OfType<ArtifactReportData>()
            .ToList();

        var customReportData = reportData
            .OfType<ICustomReportData>()
            .Where(x => !x.BeforeStandardData)
            .ToList();

        foreach (var data in customPreReportData)
            Write(data);

        Write(logReportData);

        Write(artifactReportData);

        foreach (var data in customReportData)
            Write(data);
    }

    /// <summary>
    ///     Writes log data, grouped by severity level.
    /// </summary>
    private void Write(List<LogReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        var groups = reportData
            .GroupBy(x => x.Level)
            .OrderByDescending(x => x.Key)
            .ToList();

        Write(groups
                .FirstOrDefault(x => x.Key is LogLevel.Critical)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Critical Errors",
            "maroon");

        Write(groups
                .FirstOrDefault(x => x.Key is LogLevel.Error)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Errors",
            "maroon");

        Write(groups
                .FirstOrDefault(x => x.Key is LogLevel.Warning)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Warnings",
            "darkorange");
    }

    /// <summary>
    ///     Writes a list of log data under a specific header and style.
    /// </summary>
    private void Write(List<LogReportData> reportData, string header, string styleTag)
    {
        if (reportData.Count == 0)
            return;

        var tree = new Tree(new Text(header + "\n", new(decoration: Decoration.Underline)));

        foreach (var log in reportData)
        {
            var splitPattern = new[] { '\r', '\n' };
            var messageLines = log.Message.Split(splitPattern, StringSplitOptions.RemoveEmptyEntries);

            messageLines = messageLines
                .Select(paramService.MaskMatchingSecrets)
                .ToArray();

            var rows = new List<IRenderable>
            {
                new Markup($"[{styleTag}]{messageLines.FirstOrDefault().EscapeMarkup()}[/]"),
            };

            if (messageLines.Length > 1)
                rows.AddRange(messageLines
                    .Skip(1)
                    .Select(line => new Text(line)));

            if (log.Exception is not null)
                rows.Add(new Text(paramService.MaskMatchingSecrets(log.Exception.ToString())));

            if (rows.Count > 1 && log != reportData[^1])
                rows.Add(new Text(string.Empty));

            var node = new TreeNode(new Rows(rows));

            tree.AddNode(node);
        }

        console.Write(tree);
        console.WriteLine();
        console.WriteLine();
    }

    /// <summary>
    ///     Writes artifact data as a table.
    /// </summary>
    private void Write(List<ArtifactReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        var table = new Table()
            .AddColumn("Name", c => c.LeftAligned())
            .AddColumn("Path", c => c.LeftAligned())
            .Border(TableBorder.Minimal);

        foreach (var artifact in reportData)
            table.AddRow(FormatFreetext(artifact.Name), FormatFreetext(artifact.Path));

        console.Write(new Text("Output Artifacts", new(decoration: Decoration.Underline)));
        console.WriteLine();
        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    ///     Dispatches a custom report data item to the appropriate `Write` method.
    /// </summary>
    private void Write(ICustomReportData reportData)
    {
        switch (reportData)
        {
            case TableReportData tableReportData:
                Write(tableReportData);

                break;

            case ListReportData listReportData:
                Write(listReportData);

                break;

            case TextReportData textReportData:
                Write(textReportData);

                break;

            default:
                console.Write(new Text(FormatFreetext(reportData.ToString() ?? string.Empty)));
                console.WriteLine();

                break;
        }
    }

    /// <summary>
    ///     Writes table report data.
    /// </summary>
    private void Write(TableReportData reportData)
    {
        var table = new Table().Border(TableBorder.Minimal);

        var columnCount = reportData
            .Rows
            .Append(reportData.Header ?? [])
            .Max(row => row.Count);

        var columnAlignments = reportData
            .ColumnAlignments
            .Concat(Enumerable.Repeat(ColumnAlignment.Left, columnCount - reportData.ColumnAlignments.Count))
            .ToArray();

        var headers = reportData
                          .Header
                          ?.Concat(Enumerable.Repeat(string.Empty, columnCount - reportData.Header.Count))
                          .ToArray() ??
                      Enumerable
                          .Repeat(string.Empty, columnCount)
                          .ToArray();

        headers = headers
            .Select(FormatFreetext)
            .ToArray()!;

        for (var i = 0; i < columnCount; i++)
        {
            var column = new TableColumn(headers[i])
            {
                Alignment = columnAlignments[i] switch
                {
                    ColumnAlignment.Left => Justify.Left,
                    ColumnAlignment.Center => Justify.Center,
                    ColumnAlignment.Right => Justify.Right,
                    _ => throw new UnreachableException(),
                },
            };

            table.AddColumn(column);
        }

        if (reportData.Header is null)
            table.HideHeaders();

        foreach (var row in reportData.Rows)
        {
            var formattedRow = row
                .Select(FormatFreetext)
                .ToArray();

            table.AddRow(formattedRow.Select(x => new Text(x!)));
        }

        if (reportData.Title is not null)
        {
            var title = FormatFreetext(reportData.Title);

            console.Write(new Text(title, new(decoration: Decoration.Underline)));
            console.WriteLine();
        }

        console.Write(table);
        console.WriteLine();
    }

    /// <summary>
    ///     Writes list report data.
    /// </summary>
    private void Write(ListReportData reportData)
    {
        var rows = reportData
            .Items
            .Select(x => new Text($"{FormatFreetext(reportData.Prefix)}{FormatFreetext(x)}"))
            .ToList();

        if (reportData.Title is not null)
        {
            var title = FormatFreetext(reportData.Title);

            console.Write(new Text(title, new(decoration: Decoration.Underline)));
            console.WriteLine();
            console.WriteLine();
        }

        console.Write(new Rows(rows));
        console.WriteLine();
        console.WriteLine();
    }

    /// <summary>
    ///     Writes free-form text report data.
    /// </summary>
    private void Write(TextReportData reportData)
    {
        if (reportData.Title is not null)
        {
            var title = FormatFreetext(reportData.Title);
            console.Write(new Text(title, new(decoration: Decoration.Underline)));
            console.WriteLine();
            console.WriteLine();
        }

        var text = FormatFreetext(reportData.Text);
        console.WriteLine(text);
        console.WriteLine();
        console.WriteLine();
    }

    /// <summary>
    ///     A regular expression to find and remove emojis from text.
    /// </summary>
    [GeneratedRegex(
        @"([\u2700-\u27BF]|[\uE000-\uF8FF]|\uD83C[\uDC00-\uDFFF]|\uD83D[\uDC00-\uDFFF]|[\u2011-\u26FF]|\uD83E[\uDD10-\uDDFF])")]
    private static partial Regex EmojiRegex();
}

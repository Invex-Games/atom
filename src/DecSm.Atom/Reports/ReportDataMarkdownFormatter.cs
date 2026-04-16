namespace DecSm.Atom.Reports;

/// <summary>
///     A static class that provides functionality to format a collection of <see cref="IReportData" /> objects
///     into a GitHub Flavored Markdown string.
/// </summary>
/// <remarks>
///     This formatter organizes report data into a structured Markdown document, handling logs, artifacts, tables,
///     lists, and free-form text. It uses GitHub-specific features like callouts and collapsible sections for a rich
///     presentation.
/// </remarks>
[PublicAPI]
public static class ReportDataMarkdownFormatter
{
    /// <summary>
    ///     Converts a collection of report data into a formatted Markdown string.
    /// </summary>
    /// <param name="reportData">A read-only list of <see cref="IReportData" /> objects to be formatted.</param>
    /// <returns>A string containing the complete Markdown-formatted report.</returns>
    public static string Write(IReadOnlyList<IReportData> reportData)
    {
        var builder = new StringBuilder();

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
            Write(builder, data);

        Write(builder, logReportData);

        Write(builder, artifactReportData);

        foreach (var data in customReportData)
            Write(builder, data);

        return builder.ToString();
    }

    /// <summary>
    ///     Writes log data to the builder, grouped by severity.
    /// </summary>
    private static void Write(StringBuilder builder, List<LogReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        var groups = reportData
            .GroupBy(x => x.Level)
            .OrderByDescending(x => x.Key)
            .ToList();

        Write(builder,
            groups
                .FirstOrDefault(x => x.Key is LogLevel.Critical)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Critical Errors",
            "CAUTION");

        Write(builder,
            groups
                .FirstOrDefault(x => x.Key is LogLevel.Error)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Errors",
            "CAUTION");

        Write(builder,
            groups
                .FirstOrDefault(x => x.Key is LogLevel.Warning)
                ?.OrderBy(x => x.Timestamp)
                .ToList() ?? [],
            "Warnings",
            "WARNING");
    }

    /// <summary>
    ///     Writes a list of log data under a specific header using a GitHub callout style.
    /// </summary>
    private static void Write(StringBuilder builder, List<LogReportData> reportData, string header, string infoType)
    {
        if (reportData.Count == 0)
            return;

        builder.AppendLine($"### {header}");
        builder.AppendLine();

        foreach (var log in reportData)
        {
            builder.AppendLine($"> [!{infoType}]");
            builder.AppendLine($"> {log.Message}");

            if (log.Exception is not null)
            {
                builder.AppendLine("> <details>");
                builder.AppendLine("> <summary>Exception</summary>");
                builder.AppendLine("> ");
                builder.AppendLine($"> `{log.Exception}`");
                builder.AppendLine("> ");
                builder.AppendLine("> </details>");
            }

            builder.AppendLine();
        }

        builder.AppendLine();
    }

    /// <summary>
    ///     Writes artifact data as a Markdown list of links.
    /// </summary>
    private static void Write(StringBuilder builder, List<ArtifactReportData> reportData)
    {
        if (reportData.Count == 0)
            return;

        builder.AppendLine("### Output Artifacts");
        builder.AppendLine();

        foreach (var artifactReportData in reportData)
            builder.AppendLine($"- [{artifactReportData.Name}]({artifactReportData.Path})");

        builder.AppendLine();
        builder.AppendLine();
    }

    /// <summary>
    ///     Dispatches a custom report data item to the appropriate `Write` method.
    /// </summary>
    private static void Write(StringBuilder builder, ICustomReportData reportData)
    {
        switch (reportData)
        {
            case TableReportData tableReportData:
                Write(builder, tableReportData);

                break;

            case ListReportData listReportData:
                Write(builder, listReportData);

                break;

            case TextReportData textReportData:
                Write(builder, textReportData);

                break;

            default:
                builder.AppendLine(reportData.ToString() ?? string.Empty);
                builder.AppendLine();

                break;
        }
    }

    /// <summary>
    ///     Writes table report data as a Markdown table.
    /// </summary>
    private static void Write(StringBuilder builder, TableReportData reportData)
    {
        builder.AppendLine($"### {reportData.Title}");
        builder.AppendLine();

        var columnCount = reportData
            .Rows
            .Append(reportData.Header ?? [])
            .Max(row => row.Count);

        var joinedRows = reportData
            .Rows
            .Prepend(Enumerable.Repeat("-", columnCount))
            .Prepend(reportData.Header ?? Enumerable.Repeat(string.Empty, columnCount))
            .Select(row =>
            {
                var rowArray = row.ToArray();

                return $"| {string.Join(" | ", rowArray)} |";
            })
            .ToList();

        foreach (var row in joinedRows)
            builder.AppendLine(row);

        builder.AppendLine();
        builder.AppendLine();
    }

    /// <summary>
    ///     Writes list report data as a Markdown list.
    /// </summary>
    private static void Write(StringBuilder builder, ListReportData reportData)
    {
        builder.AppendLine($"### {reportData.Title}");
        builder.AppendLine();

        foreach (var item in reportData.Items)
            builder.AppendLine($"{reportData.Prefix}{item}");

        builder.AppendLine();
        builder.AppendLine();
    }

    /// <summary>
    ///     Writes free-form text report data.
    /// </summary>
    private static void Write(StringBuilder builder, TextReportData reportData)
    {
        builder.AppendLine($"### {reportData.Title}");
        builder.AppendLine();

        builder.AppendLine(reportData.Text);
        builder.AppendLine();
        builder.AppendLine();
    }
}

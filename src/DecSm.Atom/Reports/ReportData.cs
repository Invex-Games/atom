namespace DecSm.Atom.Reports;

/// <summary>
///     Defines the base interface for all data types that can be included in a build report.
/// </summary>
/// <remarks>
///     This interface serves as a marker for data collected during a build, such as logs, artifacts, or custom data,
///     which can then be rendered by a report writer.
/// </remarks>
[PublicAPI]
public interface IReportData;

/// <summary>
///     Defines an interface for custom report data, providing control over its rendering position.
/// </summary>
[PublicAPI]
public interface ICustomReportData : IReportData
{
    /// <summary>
    ///     Gets a value indicating whether this data should be rendered before standard report data (logs and artifacts).
    /// </summary>
    bool BeforeStandardData { get; }
}

/// <summary>
///     Represents a log message captured for inclusion in a build report.
/// </summary>
/// <param name="Message">The log message text.</param>
/// <param name="Exception">The associated exception, if any.</param>
/// <param name="Level">The severity level of the log entry.</param>
/// <param name="Timestamp">The time the log entry was created.</param>
/// <remarks>
///     This data is automatically collected by <see cref="ReportLogger" /> for messages at Warning level and above.
///     Secrets are automatically masked.
/// </remarks>
[PublicAPI]
public sealed record LogReportData(string Message, Exception? Exception, LogLevel Level, DateTimeOffset Timestamp)
    : IReportData;

/// <summary>
///     Represents a build artifact (an output file) for inclusion in a build report.
/// </summary>
/// <param name="Name">The display name of the artifact.</param>
/// <param name="Path">The file system path to the artifact.</param>
[PublicAPI]
public sealed record ArtifactReportData(string Name, string Path) : IReportData;

/// <summary>
///     Represents structured tabular data for a build report.
/// </summary>
/// <param name="Rows">The data rows, where each row is a collection of cell values.</param>
/// <remarks>
///     Ideal for presenting structured information like test summaries or code coverage statistics.
/// </remarks>
[PublicAPI]
public sealed record TableReportData(IReadOnlyList<IReadOnlyList<string>> Rows) : ICustomReportData
{
    /// <summary>
    ///     Gets an optional title to display above the table.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets the column headers for the table. If null, headers are hidden.
    /// </summary>
    public IReadOnlyList<string>? Header { get; init; }

    /// <summary>
    ///     Gets the alignment for each column. Defaults to left alignment if not specified.
    /// </summary>
    public IReadOnlyList<ColumnAlignment> ColumnAlignments { get; init; } = [];

    /// <inheritdoc />
    public bool BeforeStandardData { get; init; }
}

/// <summary>
///     Represents an unordered list of items for a build report.
/// </summary>
/// <param name="Items">The list items to display.</param>
[PublicAPI]
public sealed record ListReportData(IReadOnlyList<string> Items) : ICustomReportData
{
    /// <summary>
    ///     Gets an optional title to display above the list.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    ///     Gets the prefix to prepend to each list item. Defaults to "- ".
    /// </summary>
    public string Prefix { get; init; } = "- ";

    /// <inheritdoc />
    public bool BeforeStandardData { get; init; }
}

/// <summary>
///     Represents free-form text content for a build report.
/// </summary>
/// <param name="Text">The text content to display.</param>
[PublicAPI]
public sealed record TextReportData(string Text) : ICustomReportData
{
    /// <summary>
    ///     Gets an optional title to display above the text.
    /// </summary>
    public string? Title { get; init; }

    /// <inheritdoc />
    public bool BeforeStandardData { get; init; }
}

/// <summary>
///     Specifies the horizontal alignment for table columns.
/// </summary>
[PublicAPI]
public enum ColumnAlignment
{
    /// <summary>
    ///     Aligns content to the left.
    /// </summary>
    Left,

    /// <summary>
    ///     Aligns content to the center.
    /// </summary>
    Center,

    /// <summary>
    ///     Aligns content to the right.
    /// </summary>
    Right,
}

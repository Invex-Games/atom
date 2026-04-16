namespace DecSm.Atom.Module.Dotnet.Model;

/// <summary>
///     Represents the root model for code coverage summary data, typically deserialized from a JSON report.
/// </summary>
[PublicAPI]
public sealed record CoverageModel
{
    /// <summary>
    ///     Gets the summary of the code coverage report.
    /// </summary>
    [JsonPropertyName("summary")]
    public CoverageSummary Summary { get; init; } = new();
}

/// <summary>
///     Represents a detailed summary of code coverage metrics.
/// </summary>
[PublicAPI]
public sealed record CoverageSummary
{
    /// <summary>
    ///     Gets the date and time when the coverage report was generated.
    /// </summary>
    [JsonPropertyName("generatedon")]
    public DateTime GeneratedOn { get; init; }

    /// <summary>
    ///     Gets the name of the parser used to generate the coverage report.
    /// </summary>
    [JsonPropertyName("parser")]
    public string Parser { get; init; } = string.Empty;

    /// <summary>
    ///     Gets the total number of assemblies covered.
    /// </summary>
    [JsonPropertyName("assemblies")]
    public int Assemblies { get; init; }

    /// <summary>
    ///     Gets the total number of classes covered.
    /// </summary>
    [JsonPropertyName("classes")]
    public int Classes { get; init; }

    /// <summary>
    ///     Gets the total number of files covered.
    /// </summary>
    [JsonPropertyName("files")]
    public int Files { get; init; }

    /// <summary>
    ///     Gets the number of lines that have been covered by tests.
    /// </summary>
    [JsonPropertyName("coveredlines")]
    public int CoveredLines { get; init; }

    /// <summary>
    ///     Gets the number of lines that have not been covered by tests.
    /// </summary>
    [JsonPropertyName("uncoveredlines")]
    public int UncoveredLines { get; init; }

    /// <summary>
    ///     Gets the total number of lines that could potentially be covered by tests.
    /// </summary>
    [JsonPropertyName("coverablelines")]
    public int CoverableLines { get; init; }

    /// <summary>
    ///     Gets the total number of lines in the codebase.
    /// </summary>
    [JsonPropertyName("totallines")]
    public int TotalLines { get; init; }

    /// <summary>
    ///     Gets the percentage of lines covered by tests.
    /// </summary>
    [JsonPropertyName("linecoverage")]
    public double LineCoverage { get; init; }

    /// <summary>
    ///     Gets the number of branches that have been covered by tests.
    /// </summary>
    [JsonPropertyName("coveredbranches")]
    public int CoveredBranches { get; init; }

    /// <summary>
    ///     Gets the total number of branches in the codebase.
    /// </summary>
    [JsonPropertyName("totalbranches")]
    public int TotalBranches { get; init; }

    /// <summary>
    ///     Gets the percentage of branches covered by tests.
    /// </summary>
    [JsonPropertyName("branchcoverage")]
    public double BranchCoverage { get; init; }

    /// <summary>
    ///     Gets the number of methods that have been covered by tests.
    /// </summary>
    [JsonPropertyName("coveredmethods")]
    public int CoveredMethods { get; init; }

    /// <summary>
    ///     Gets the total number of methods in the codebase.
    /// </summary>
    [JsonPropertyName("totalmethods")]
    public int TotalMethods { get; init; }

    /// <summary>
    ///     Gets the percentage of methods covered by tests.
    /// </summary>
    [JsonPropertyName("methodcoverage")]
    public double MethodCoverage { get; init; }
}

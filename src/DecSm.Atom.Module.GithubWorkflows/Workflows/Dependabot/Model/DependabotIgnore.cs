namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Ignore rule for Dependabot updates.
/// </summary>
[PublicAPI]
public sealed record DependabotIgnore
{
    /// <summary>
    ///     Use to ignore updates for dependencies with matching names, optionally using * to match zero or more characters.
    /// </summary>
    public required string? DependencyName { get; init; }

    /// <summary>
    ///     Use to ignore types of updates.
    /// </summary>
    public required IReadOnlyList<SemverUpdateType>? UpdateTypes { get; init; }

    /// <summary>
    ///     Use to ignore specific versions or ranges of versions.
    /// </summary>
    public required DependabotVersions? Versions { get; init; }
}

/// <summary>
///     Semantic version update type.
/// </summary>
[PublicAPI]
public enum SemverUpdateType
{
    VersionUpdateSemverMajor,
    VersionUpdateSemverMinor,
    VersionUpdateSemverPatch,
}

/// <summary>
///     Versions to ignore. Can be a single version string or a list of version strings.
/// </summary>
[PublicAPI]
[Union]
public partial record DependabotVersions
{
    public partial record Single(string Version);

    public partial record Multiple(IReadOnlyList<string> Versions);
}

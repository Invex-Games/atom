namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Allow rule for Dependabot updates.
/// </summary>
[UnstableAPI]
public sealed record DependabotAllow
{
    /// <summary>
    ///     The dependency name to allow.
    /// </summary>
    public required string? DependencyName { get; init; }

    /// <summary>
    ///     The dependency type to allow.
    /// </summary>
    public required DependencyType? DependencyType { get; init; }
}

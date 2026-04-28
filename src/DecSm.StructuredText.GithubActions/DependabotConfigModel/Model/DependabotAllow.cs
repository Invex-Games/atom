namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Allow rule for Dependabot updates.
/// </summary>
[PublicAPI]
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

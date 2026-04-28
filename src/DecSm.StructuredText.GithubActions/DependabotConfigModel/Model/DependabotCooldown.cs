namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Cooldown configuration for dependency updates.
/// </summary>
[PublicAPI]
public sealed record DependabotCooldown
{
    /// <summary>
    ///     Default cooldown period for dependencies without specific rules (optional).
    /// </summary>
    public int? DefaultDays { get; init; }

    /// <summary>
    ///     Cooldown period for major version updates (optional, applies only to package managers supporting SemVer).
    /// </summary>
    public int? SemverMajorDays { get; init; }

    /// <summary>
    ///     Cooldown period for minor version updates (optional, applies only to package managers supporting SemVer).
    /// </summary>
    public int? SemverMinorDays { get; init; }

    /// <summary>
    ///     Cooldown period for patch version updates (optional, applies only to package managers supporting SemVer).
    /// </summary>
    public int? SemverPatchDays { get; init; }

    /// <summary>
    ///     List of dependencies to apply cooldown. Supports wildcards (*).
    /// </summary>
    public IReadOnlyList<string>? Include { get; init; }

    /// <summary>
    ///     List of dependencies excluded from cooldown. Supports wildcards (*).
    /// </summary>
    public IReadOnlyList<string>? Exclude { get; init; }
}

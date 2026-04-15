namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     How to update manifest version requirements.
/// </summary>
[UnstableAPI]
public enum VersioningStrategy
{
    /// <summary>
    ///     Try to differentiate between apps and libraries. Use 'increase' for apps and 'widen' for libraries.
    /// </summary>
    Auto,

    /// <summary>
    ///     Always increase the minimum version requirement to match the new version.
    /// </summary>
    Increase,

    /// <summary>
    ///     Leave the constraint if the original constraint allows the new version, otherwise, bump the constraint.
    /// </summary>
    IncreaseIfNecessary,

    /// <summary>
    ///     Only create pull requests to update lockfiles. Ignore any new versions that would require package manifest changes.
    /// </summary>
    LockfileOnly,

    /// <summary>
    ///     Widen the allowed version requirements to include both the new and old versions, when possible.
    /// </summary>
    Widen,
}

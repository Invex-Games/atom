namespace DecSm.Atom.Module.GitVersion;

/// <summary>
///     Represents a workflow option to enable or disable the use of GitVersion for determining the build ID.
/// </summary>
/// <remarks>
///     When this option is enabled, the build ID will be derived from GitVersion's output,
///     typically reflecting the semantic versioning of the repository.
/// </remarks>
[PublicAPI]
public sealed record UseGitVersionForBuildId : ToggleWorkflowOption<UseGitVersionForBuildId>;

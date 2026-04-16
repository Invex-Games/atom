namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Package ecosystem configuration for Dependabot updates.
/// </summary>
[UnstableAPI]
public sealed record DependabotUpdate
{
    /// <summary>
    ///     Customize which updates are allowed.
    /// </summary>
    public IReadOnlyList<DependabotAllow>? Allow { get; init; }

    /// <summary>
    ///     Assignees to set on pull requests.
    /// </summary>
    public IReadOnlyList<string>? Assignees { get; init; }

    /// <summary>
    ///     Commit message preferences.
    /// </summary>
    public DependabotCommitMessage? CommitMessage { get; init; }

    /// <summary>
    ///     Defines a cooldown period for dependency updates.
    /// </summary>
    public DependabotCooldown? Cooldown { get; init; }

    /// <summary>
    ///     Locations of package manifests.
    /// </summary>
    public IReadOnlyList<string> Directories { get; init; } = [];

    /// <summary>
    ///     Location of package manifests.
    /// </summary>
    public string? Directory { get; init; }

    /// <summary>
    ///     List of file paths to exclude from dependency updates.
    /// </summary>
    public IReadOnlyList<string>? ExcludePaths { get; init; }

    /// <summary>
    ///     Configure groups for dependencies.
    /// </summary>
    public IReadOnlyDictionary<string, DependabotGroup>? Groups { get; init; }

    /// <summary>
    ///     Ignore certain dependencies or versions.
    /// </summary>
    public IReadOnlyList<DependabotIgnore>? Ignore { get; init; }

    /// <summary>
    ///     Allow or deny code execution in manifest files.
    /// </summary>
    public InsecureExternalCodeExecution? InsecureExternalCodeExecution { get; init; }

    /// <summary>
    ///     Labels to set on pull requests.
    /// </summary>
    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>
    ///     Associate all pull requests raised for a package manager with a milestone.
    /// </summary>
    public int? Milestone { get; init; }

    /// <summary>
    ///     A name for the update configuration.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    ///     Limit number of open pull requests for version updates.
    /// </summary>
    public int? OpenPullRequestsLimit { get; init; }

    /// <summary>
    ///     Package manager to use.
    /// </summary>
    public required string PackageEcosystem { get; init; }

    /// <summary>
    ///     Pull request branch name preferences.
    /// </summary>
    public DependabotPullRequestBranchName? PullRequestBranchName { get; init; }

    /// <summary>
    ///     Disable automatic rebasing.
    /// </summary>
    public RebaseStrategy? RebaseStrategy { get; init; }

    /// <summary>
    ///     Registries to use for this update configuration. Can be a list of registry names or "*" for all.
    /// </summary>
    public DependabotRegistries? Registries { get; init; }

    /// <summary>
    ///     Schedule preferences.
    /// </summary>
    public DependabotSchedule? Schedule { get; init; }

    /// <summary>
    ///     Specify a different branch for manifest files and for pull requests.
    /// </summary>
    public string? TargetBranch { get; init; }

    /// <summary>
    ///     Tell Dependabot to vendor dependencies when updating them.
    /// </summary>
    public bool? Vendor { get; init; }

    /// <summary>
    ///     How to update manifest version requirements.
    /// </summary>
    public VersioningStrategy? VersioningStrategy { get; init; }

    /// <summary>
    ///     Array of dependency patterns to include in a multi-ecosystem group.
    /// </summary>
    public IReadOnlyList<string>? Patterns { get; init; }

    /// <summary>
    ///     String identifier linking this ecosystem to a multi-ecosystem group.
    /// </summary>
    public string? MultiEcosystemGroup { get; init; }
}

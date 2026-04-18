namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Multi-ecosystem group configuration that spans multiple package ecosystems.
/// </summary>
[PublicAPI]
public sealed record DependabotMultiEcosystemGroup
{
    /// <summary>
    ///     Schedule preferences for the group.
    /// </summary>
    public required DependabotSchedule Schedule { get; init; }

    /// <summary>
    ///     Labels to set on pull requests (additive - merges with ecosystem-level labels).
    /// </summary>
    public IReadOnlyList<string>? Labels { get; init; }

    /// <summary>
    ///     Assignees to set on pull requests (additive - merges with ecosystem-level assignees).
    /// </summary>
    public IReadOnlyList<string>? Assignees { get; init; }

    /// <summary>
    ///     Associate all pull requests raised for this group with a milestone.
    /// </summary>
    public int? Milestone { get; init; }

    /// <summary>
    ///     Specify a different branch for manifest files and for pull requests.
    /// </summary>
    public string? TargetBranch { get; init; }

    /// <summary>
    ///     Commit message preferences for the group.
    /// </summary>
    public DependabotCommitMessage? CommitMessage { get; init; }

    /// <summary>
    ///     Pull request branch name preferences for the group.
    /// </summary>
    public DependabotPullRequestBranchName? PullRequestBranchName { get; init; }

    /// <summary>
    ///     Limit number of open pull requests for version updates.
    /// </summary>
    public int? OpenPullRequestsLimit { get; init; }

    /// <summary>
    ///     Specify the semantic versioning update types for the group.
    /// </summary>
    public IReadOnlyList<GroupUpdateType>? UpdateTypes { get; init; }

    /// <summary>
    ///     Specify a dependency type to be included in the group.
    /// </summary>
    public GroupDependencyType? DependencyType { get; init; }

    /// <summary>
    ///     Exclude certain dependencies from the group.
    /// </summary>
    public IReadOnlyList<string>? ExcludePatterns { get; init; }
}

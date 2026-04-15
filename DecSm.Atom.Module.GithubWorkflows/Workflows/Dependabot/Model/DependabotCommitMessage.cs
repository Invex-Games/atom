namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Commit message preferences for Dependabot.
/// </summary>
[UnstableAPI]
public sealed record DependabotCommitMessage
{
    /// <summary>
    ///     A prefix for all commit messages.
    /// </summary>
    public required string? Prefix { get; init; }

    /// <summary>
    ///     A separate prefix for all commit messages that update dependencies in the Development dependency group.
    /// </summary>
    public required string? PrefixDevelopment { get; init; }

    /// <summary>
    ///     Specifies that any prefix is followed by a list of the dependencies updated in the commit.
    /// </summary>
    public required CommitMessageInclude? Include { get; init; }
}

/// <summary>
///     Commit message include option.
/// </summary>
[UnstableAPI]
public enum CommitMessageInclude
{
    Scope,
}

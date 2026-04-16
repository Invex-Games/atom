namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot.Model;

/// <summary>
///     Pull request branch name preferences.
/// </summary>
[UnstableAPI]
public sealed record DependabotPullRequestBranchName
{
    /// <summary>
    ///     Change separator for PR branch name.
    /// </summary>
    public required BranchNameSeparator Separator { get; init; }
}

/// <summary>
///     Branch name separator.
/// </summary>
[UnstableAPI]
public enum BranchNameSeparator
{
    Hyphen,
    Underscore,
    Slash,
}

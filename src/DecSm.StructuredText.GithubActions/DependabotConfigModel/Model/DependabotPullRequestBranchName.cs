namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Pull request branch name preferences.
/// </summary>
[PublicAPI]
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
[PublicAPI]
public enum BranchNameSeparator
{
    Hyphen,
    Underscore,
    Slash,
}

namespace DecSm.StructuredText.GithubActions.DependabotConfigModel.Model;

/// <summary>
///     Rebase strategy for Dependabot.
/// </summary>
[PublicAPI]
public enum RebaseStrategy
{
    /// <summary>
    ///     Dependabot will rebase open pull requests when changes are detected.
    /// </summary>
    Auto,

    /// <summary>
    ///     Automatic rebasing is disabled.
    /// </summary>
    Disabled,
}

namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Provides common GitHub-related parameters and functionality for DecSm.Atom builds.
/// </summary>
/// <remarks>
///     Implementing this interface makes the GitHub token available as a secret parameter,
///     which is often required for interacting with the GitHub API within workflows.
/// </remarks>
[PublicAPI]
public interface IGithubHelper : IBuildAccessor
{
    /// <summary>
    ///     Gets the GitHub token, typically used for authentication with the GitHub API.
    /// </summary>
    /// <remarks>
    ///     This is a secret parameter and should be provided securely, e.g., via GitHub Actions secrets.
    ///     It grants permissions to interact with the repository and other GitHub resources.
    /// </remarks>
    [SecretDefinition("github-token", "Github Token")]
    string GithubToken => GetParam(() => GithubToken)!;
}

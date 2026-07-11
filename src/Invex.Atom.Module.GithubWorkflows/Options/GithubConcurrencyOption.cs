namespace Invex.Atom.Module.GithubWorkflows.Options;

/// <summary>
///     Configures concurrency for a GitHub Actions workflow.
/// </summary>
/// <param name="Group">The concurrency group key.</param>
/// <param name="CancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
[PublicAPI]
public sealed record GithubConcurrencyOption(TextExpression Group, TextExpression? CancelInProgress = null)
    : IBuildOption;

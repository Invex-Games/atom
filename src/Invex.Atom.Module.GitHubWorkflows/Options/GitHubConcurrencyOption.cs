namespace Invex.Atom.Module.GitHubWorkflows.Options;

/// <summary>
///     Configures concurrency for a GitHub Actions workflow.
/// </summary>
/// <param name="Group">The concurrency group key.</param>
/// <param name="CancelInProgress">Whether to cancel an in-progress workflow in the same group.</param>
[PublicAPI]
public sealed record GitHubConcurrencyOption(TextExpression Group, TextExpression? CancelInProgress = null)
    : IBuildOption;

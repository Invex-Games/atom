namespace Invex.Atom.Module.DevOpsWorkflows.Workflows.Options;

/// <summary>
///     Configures Azure Pipelines pull request behavior.
/// </summary>
/// <param name="AutoCancel">
///     Whether a new commit to a pull request cancels its in-progress pipeline run.
/// </param>
[PublicAPI]
public sealed record DevOpsPullRequestOption(bool AutoCancel) : IBuildOption;

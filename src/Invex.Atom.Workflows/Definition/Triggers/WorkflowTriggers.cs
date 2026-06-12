namespace Invex.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Provides factories and presets for common workflow triggers, and serves as an extension anchor
///     for platform-specific trigger factories.
/// </summary>
[PublicAPI]
public static class WorkflowTriggers
{
    /// <summary>
    ///     Gets a trigger that allows the workflow to be started manually, without inputs.
    /// </summary>
    public static ManualTrigger Manual => field ??= new();

    /// <summary>
    ///     Gets a trigger that starts the workflow for pull requests targeting the <c>main</c> branch.
    /// </summary>
    public static GitPullRequestTrigger PullIntoMain =>
        field ??= new()
        {
            IncludedBranches = ["main"],
        };

    /// <summary>
    ///     Gets a trigger that starts the workflow for pushes to the <c>main</c> branch.
    /// </summary>
    public static GitPushTrigger PushToMain =>
        field ??= new()
        {
            IncludedBranches = ["main"],
        };

    /// <summary>
    ///     Creates a trigger that starts the workflow for pull requests targeting the specified branches.
    /// </summary>
    /// <param name="includedBranches">The target branches that activate the trigger.</param>
    /// <returns>A <see cref="GitPullRequestTrigger" />.</returns>
    public static GitPullRequestTrigger PullInto(params string[] includedBranches) =>
        new()
        {
            IncludedBranches = includedBranches.ToList(),
        };

    /// <summary>
    ///     Creates a trigger that starts the workflow for pushes to the specified branches.
    /// </summary>
    /// <param name="includedBranches">The branches that activate the trigger.</param>
    /// <returns>A <see cref="GitPushTrigger" />.</returns>
    public static GitPushTrigger PushTo(params string[] includedBranches) =>
        new()
        {
            IncludedBranches = includedBranches.ToList(),
        };

    /// <summary>
    ///     Creates a trigger that allows the workflow to be started manually with the specified inputs.
    /// </summary>
    /// <param name="inputs">The inputs that can be supplied when starting the workflow.</param>
    /// <returns>A <see cref="ManualTrigger" />.</returns>
    public static ManualTrigger ManualWithInputs(params ManualInput[] inputs) =>
        new(inputs.ToList());

    /// <summary>
    ///     Creates a fully-configured pull request trigger.
    /// </summary>
    /// <param name="includedBranches">The target branches that activate the trigger.</param>
    /// <param name="excludedBranches">The target branches that are excluded from the trigger.</param>
    /// <param name="includedPaths">The changed file paths that activate the trigger.</param>
    /// <param name="excludedPaths">The changed file paths that are excluded from the trigger.</param>
    /// <param name="types">The pull request event types that activate the trigger (e.g., "opened").</param>
    /// <returns>A <see cref="GitPullRequestTrigger" />.</returns>
    public static GitPullRequestTrigger PullRequest(
        IReadOnlyList<string> includedBranches,
        IReadOnlyList<string> excludedBranches,
        IReadOnlyList<string> includedPaths,
        IReadOnlyList<string> excludedPaths,
        IReadOnlyList<string> types) =>
        new()
        {
            IncludedBranches = includedBranches,
            ExcludedBranches = excludedBranches,
            IncludedPaths = includedPaths,
            ExcludedPaths = excludedPaths,
            Types = types,
        };

    /// <summary>
    ///     Creates a fully-configured push trigger.
    /// </summary>
    /// <param name="includedBranches">The branches that activate the trigger.</param>
    /// <param name="excludedBranches">The branches that are excluded from the trigger.</param>
    /// <param name="includedPaths">The changed file paths that activate the trigger.</param>
    /// <param name="excludedPaths">The changed file paths that are excluded from the trigger.</param>
    /// <param name="includedTags">The tags that activate the trigger.</param>
    /// <param name="excludedTags">The tags that are excluded from the trigger.</param>
    /// <returns>A <see cref="GitPushTrigger" />.</returns>
    public static GitPushTrigger Push(
        IReadOnlyList<string> includedBranches,
        IReadOnlyList<string> excludedBranches,
        IReadOnlyList<string> includedPaths,
        IReadOnlyList<string> excludedPaths,
        IReadOnlyList<string> includedTags,
        IReadOnlyList<string> excludedTags) =>
        new()
        {
            IncludedBranches = includedBranches,
            ExcludedBranches = excludedBranches,
            IncludedPaths = includedPaths,
            ExcludedPaths = excludedPaths,
            IncludedTags = includedTags,
            ExcludedTags = excludedTags,
        };
}

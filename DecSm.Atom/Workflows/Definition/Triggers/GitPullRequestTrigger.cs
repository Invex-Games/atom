namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a workflow trigger that activates on Git pull request events.
/// </summary>
/// <remarks>
///     This trigger provides fine-grained control over activation conditions through branch patterns,
///     file path filters, and pull request event types.
/// </remarks>
/// <example>
///     <code>
/// // Trigger on pull requests into the main branch
/// var mainPrTrigger = GitPullRequestTrigger.IntoMain;
/// // Trigger on pull requests that modify files in the 'src' directory
/// var srcPrTrigger = new GitPullRequestTrigger
/// {
///     IncludedPaths = ["src/**"]
/// };
///     </code>
/// </example>
[PublicAPI]
public sealed record GitPullRequestTrigger : IWorkflowTrigger
{
    /// <summary>
    ///     Gets the list of branch patterns that should trigger the workflow. Supports glob patterns.
    /// </summary>
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets the list of branch patterns that should NOT trigger the workflow. Takes precedence over
    ///     <see cref="IncludedBranches" />.
    /// </summary>
    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets the list of file path patterns that must be affected for the trigger to activate. Supports glob patterns.
    /// </summary>
    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets the list of file path patterns that should NOT activate the trigger. Takes precedence over
    ///     <see cref="IncludedPaths" />.
    /// </summary>
    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets the list of pull request event types that should activate this trigger (e.g., "opened", "synchronize").
    /// </summary>
    public IReadOnlyList<string> Types { get; init; } = [];
}

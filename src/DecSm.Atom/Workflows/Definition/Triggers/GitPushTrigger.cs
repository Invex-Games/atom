namespace DecSm.Atom.Workflows.Definition.Triggers;

/// <summary>
///     Represents a workflow trigger that activates when code is pushed to a Git repository.
/// </summary>
/// <remarks>
///     This trigger supports filtering by branches, file paths, and tags to control workflow execution.
///     When filter lists are empty, no filtering is applied for that category.
/// </remarks>
/// <example>
///     <code>
/// // Trigger on pushes to the main branch only
/// var mainTrigger = GitPushTrigger.ToMain;
/// // Trigger on pushes to any branch, excluding documentation changes
/// var featureTrigger = new GitPushTrigger
/// {
///     ExcludedPaths = ["docs/**", "*.md"]
/// };
///     </code>
/// </example>
[PublicAPI]
public sealed record GitPushTrigger : IWorkflowTrigger
{
    /// <summary>
    ///     Gets or sets the list of branch patterns that should trigger the workflow.
    /// </summary>
    public IReadOnlyList<string> IncludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of branch patterns that should NOT trigger the workflow. Takes precedence over
    ///     <see cref="IncludedBranches" />.
    /// </summary>
    public IReadOnlyList<string> ExcludedBranches { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of file path patterns that should trigger the workflow.
    /// </summary>
    public IReadOnlyList<string> IncludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of file path patterns that should NOT trigger the workflow. Takes precedence over
    ///     <see cref="IncludedPaths" />.
    /// </summary>
    public IReadOnlyList<string> ExcludedPaths { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of tag patterns that should trigger the workflow.
    /// </summary>
    public IReadOnlyList<string> IncludedTags { get; init; } = [];

    /// <summary>
    ///     Gets or sets the list of tag patterns that should NOT trigger the workflow. Takes precedence over
    ///     <see cref="IncludedTags" />.
    /// </summary>
    public IReadOnlyList<string> ExcludedTags { get; init; } = [];
}

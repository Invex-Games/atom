namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Triggers;

/// <summary>
///     A pull request trigger specifies which branches cause a pull request build to run.
/// </summary>
/// <remarks>
///     Supports three implementations:
///     - None: Disable PR triggers
///     - BranchList: List of branches that trigger a run
///     - Full: Full syntax for complete control over autoCancel, branches, paths, and drafts
/// </remarks>
[PublicAPI]
[Union]
public partial record Pr
{
    /// <summary>
    ///     Disable PR triggers.
    /// </summary>
    public sealed partial record None;

    /// <summary>
    ///     List of branches that trigger a pull request run.
    /// </summary>
    public sealed partial record BranchList
    {
        /// <summary>
        ///     Branch names that trigger a run.
        /// </summary>
        public required WorkflowExpressionCollection Branches { get; init; }
    }

    /// <summary>
    ///     Full syntax for complete control over PR triggers.
    /// </summary>
    public sealed partial record Full
    {
        /// <summary>
        ///     Whether to cancel running PR builds when a new commit lands in the branch.
        /// </summary>
        public WorkflowExpression? AutoCancel { get; init; }

        /// <summary>
        ///     Branch names to include or exclude for triggering a run.
        /// </summary>
        public IncludeExcludeFilters? Branches { get; init; }

        /// <summary>
        ///     File paths to include or exclude for triggering a run.
        /// </summary>
        public IncludeExcludeFilters? Paths { get; init; }

        /// <summary>
        ///     Whether to build pull requests from forks.
        /// </summary>
        public WorkflowExpression? Drafts { get; init; }
    }
}

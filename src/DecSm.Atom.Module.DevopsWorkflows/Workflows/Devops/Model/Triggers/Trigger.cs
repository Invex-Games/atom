namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Triggers;

/// <summary>
///     A push trigger specifies which branches cause a continuous integration build to run.
/// </summary>
/// <remarks>
///     Supports three implementations:
///     - None: Disable CI triggers
///     - BranchList: List of branches that trigger a run
///     - Full: Full syntax for complete control over batch, branches, paths, and tags
/// </remarks>
[PublicAPI]
[Union]
public partial record Trigger
{
    /// <summary>
    ///     Disable CI triggers.
    /// </summary>
    public sealed partial record None;

    /// <summary>
    ///     List of branches that trigger a run.
    /// </summary>
    public sealed partial record BranchList
    {
        /// <summary>
        ///     Branch names that trigger a run.
        /// </summary>
        public required WorkflowExpressionCollection<string> Branches { get; init; }
    }

    /// <summary>
    ///     Full syntax for complete control over CI triggers.
    /// </summary>
    public sealed partial record Full
    {
        /// <summary>
        ///     Whether to batch changes per branch.
        /// </summary>
        public WorkflowExpression<bool>? Batch { get; init; }

        /// <summary>
        ///     Branch names to include or exclude for triggering a run.
        /// </summary>
        public IncludeExcludeFilters? Branches { get; init; }

        /// <summary>
        ///     File paths to include or exclude for triggering a run.
        /// </summary>
        public IncludeExcludeFilters? Paths { get; init; }

        /// <summary>
        ///     Tag names to include or exclude for triggering a run.
        /// </summary>
        public IncludeExcludeFilters? Tags { get; init; }
    }
}

namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Stages;

/// <summary>
///     A stage is a collection of related jobs.
/// </summary>
/// <remarks>
///     Supports two implementations:
///     - Stage: A regular stage with jobs
///     - Template: Reference to a stage template
/// </remarks>
[PublicAPI]
[Union]
public partial record Stage
{
    /// <summary>
    ///     A collection of related jobs.
    /// </summary>
    public sealed partial record StageDefinition
    {
        /// <summary>
        ///     ID of the stage.
        /// </summary>
        public required WorkflowExpression<string> StageId { get; init; }

        /// <summary>
        ///     Path to the group which the stage belongs to.
        /// </summary>
        public WorkflowExpression<string>? Group { get; init; }

        /// <summary>
        ///     Human-readable name for the stage.
        /// </summary>
        public WorkflowExpression<string>? DisplayName { get; init; }

        /// <summary>
        ///     Pool where jobs in this stage will run unless otherwise specified.
        /// </summary>
        public Pool? Pool { get; init; }

        /// <summary>
        ///     Any stages which must complete before this one.
        /// </summary>
        public WorkflowExpressionCollection<string>? DependsOn { get; init; }

        /// <summary>
        ///     Evaluate this condition expression to determine whether to run this stage.
        /// </summary>
        public WorkflowExpression<bool>? Condition { get; init; }

        /// <summary>
        ///     Stage-specific variables.
        /// </summary>
        public Variables.Variables? Variables { get; init; }

        /// <summary>
        ///     Jobs which make up the stage.
        /// </summary>
        public IReadOnlyList<Job>? Jobs { get; init; }

        /// <summary>
        ///     Behavior lock requests from this stage should exhibit in relation to other exclusive lock requests.
        ///     Valid values: "sequential" | "runLatest"
        /// </summary>
        public WorkflowExpression<string>? LockBehavior { get; init; }

        /// <summary>
        ///     Stage trigger manual or automatic (default).
        ///     Valid values: "manual" | "automatic"
        /// </summary>
        public WorkflowExpression<string>? Trigger { get; init; }

        /// <summary>
        ///     Setting false prevents the stage from being skipped. By default it's always true.
        /// </summary>
        public WorkflowExpression<bool>? IsSkippable { get; init; }

        /// <summary>
        ///     Stage related information passed from a pipeline when extending a template.
        /// </summary>
        public IReadOnlyDictionary<string, TextExpression>? TemplateContext { get; init; }
    }

    /// <summary>
    ///     Reference to a stage template.
    /// </summary>
    public sealed partial record Template
    {
        /// <summary>
        ///     Path to the template file.
        /// </summary>
        public required WorkflowExpression<string> TemplatePath { get; init; }

        /// <summary>
        ///     Parameters to pass to the template.
        /// </summary>
        public IReadOnlyDictionary<string, TextExpression>? Parameters { get; init; }
    }
}

namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops.Model.Supporting;

/// <summary>
///     Include and exclude filters for branches, paths, or tags.
/// </summary>
[PublicAPI]
public sealed record IncludeExcludeFilters
{
    /// <summary>
    ///     List of items to include.
    /// </summary>
    public WorkflowExpressionCollection? Include { get; init; }

    /// <summary>
    ///     List of items to exclude.
    /// </summary>
    public WorkflowExpressionCollection? Exclude { get; init; }
}

/// <summary>
///     Workspace options on the agent.
/// </summary>
[PublicAPI]
public sealed record Workspace
{
    /// <summary>
    ///     What to clean up before the job runs.
    ///     Valid values: "outputs" | "resources" | "all"
    /// </summary>
    public WorkflowExpression? Clean { get; init; }
}

/// <summary>
///     Extends a template.
/// </summary>
[PublicAPI]
public sealed record Extends
{
    /// <summary>
    ///     The template referenced by the pipeline to extend.
    /// </summary>
    public required WorkflowExpression Template { get; init; }

    /// <summary>
    ///     Parameters used in the extend.
    /// </summary>
    public IReadOnlyDictionary<string, WorkflowExpression>? Parameters { get; init; }
}

/// <summary>
///     Pipeline template parameter definition.
/// </summary>
[PublicAPI]
public sealed record Parameter
{
    /// <summary>
    ///     Parameter name.
    /// </summary>
    public required WorkflowExpression Name { get; init; }

    /// <summary>
    ///     Parameter display name.
    /// </summary>
    public WorkflowExpression? DisplayName { get; init; }

    /// <summary>
    ///     Parameter type.
    ///     Valid values: "string" | "number" | "boolean" | "object" | "step" | "stepList" |
    ///     "job" | "jobList" | "deployment" | "deploymentList" | "stage" | "stageList"
    /// </summary>
    public WorkflowExpression? Type { get; init; }

    /// <summary>
    ///     Default value for the parameter.
    /// </summary>
    public WorkflowExpression? Default { get; init; }

    /// <summary>
    ///     Allowed values for the parameter.
    /// </summary>
    public WorkflowExpressionCollection? Values { get; init; }
}

/// <summary>
///     Scheduled trigger (cron).
/// </summary>
[PublicAPI]
public sealed record Schedule
{
    /// <summary>
    ///     Cron expression for the schedule.
    /// </summary>
    public required WorkflowExpression Cron { get; init; }

    /// <summary>
    ///     Display name for the schedule.
    /// </summary>
    public WorkflowExpression? DisplayName { get; init; }

    /// <summary>
    ///     Branches to include or exclude for the scheduled trigger.
    /// </summary>
    public IncludeExcludeFilters? Branches { get; init; }

    /// <summary>
    ///     Whether to run the schedule if the code hasn't changed.
    /// </summary>
    public WorkflowExpression? Always { get; init; }
}

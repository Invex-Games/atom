namespace DecSm.Atom.Workflows.Model;

/// <summary>
///     Represents a workflow model, including its name, triggers, options, and jobs, used for generating CI/CD workflow
///     files.
/// </summary>
/// <param name="Name">The name of the workflow.</param>
[PublicAPI]
public sealed record WorkflowModel(string Name)
{
    /// <summary>
    ///     Gets the triggers that define when the workflow should be initiated.
    /// </summary>
    /// <remarks>
    ///     Examples include code pushes, pull requests, or scheduled events.
    /// </remarks>
    public required IReadOnlyList<IWorkflowTrigger> Triggers { get; init; }

    /// <summary>
    ///     Gets the options that configure the workflow's behavior.
    /// </summary>
    /// <remarks>
    ///     Options can include input parameters, environment variables, or other settings.
    /// </remarks>
    public required IReadOnlyList<IWorkflowOption> Options { get; init; }

    /// <summary>
    ///     Gets the jobs that define the sequence of tasks to be executed by the workflow.
    /// </summary>
    public required IReadOnlyList<WorkflowJobModel> Jobs { get; init; }
}

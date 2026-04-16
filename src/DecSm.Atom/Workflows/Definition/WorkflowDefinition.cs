namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Represents the definition of a workflow, including its name, triggers, options, targets, and types.
/// </summary>
/// <param name="Name">The name of the workflow.</param>
[PublicAPI]
public sealed record WorkflowDefinition(string Name)
{
    /// <summary>
    ///     Gets the collection of triggers that define when and how the workflow is initiated.
    /// </summary>
    public IReadOnlyList<IWorkflowTrigger> Triggers { get; init; } = [];

    /// <summary>
    ///     Gets the collection of options or parameters that can be configured for the workflow.
    /// </summary>
    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];

    /// <summary>
    ///     Gets the collection of targets that define the sequence of tasks to be executed by the workflow.
    /// </summary>
    public IReadOnlyList<WorkflowTargetDefinition> Targets { get; init; } = [];

    /// <summary>
    ///     Gets the collection of workflow types that this definition applies to (e.g., GitHub Actions, Azure DevOps).
    /// </summary>
    public IReadOnlyList<IWorkflowType> WorkflowTypes { get; init; } = [];
}

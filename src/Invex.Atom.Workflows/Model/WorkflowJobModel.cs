namespace Invex.Atom.Workflows.Model;

/// <summary>
///     Represents a job within a workflow, including its name, steps, dependencies, and configuration options.
/// </summary>
[PublicAPI]
public sealed record WorkflowJobModel
{
    /// <summary>The name of the job.</summary>
    public required string Name { get; init; }

    /// <summary>The sequence of steps to be executed in the job.</summary>
    public required WorkflowStepModel TargetStep { get; init; }

    /// <summary>
    ///     Gets the names of other jobs that must be completed before this job can start.
    /// </summary>
    public required IReadOnlyList<string> JobDependencies { get; init; }
}

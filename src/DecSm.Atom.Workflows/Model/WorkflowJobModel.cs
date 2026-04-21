namespace DecSm.Atom.Workflows.Model;

/// <summary>
///     Represents a job within a workflow, including its name, steps, dependencies, and configuration options.
/// </summary>
/// <param name="Name">The name of the job.</param>
/// <param name="TargetStep">The sequence of steps to be executed in the job.</param>
[PublicAPI]
public sealed record WorkflowJobModel(string Name, WorkflowStepModel TargetStep)
{
    /// <summary>
    ///     Gets the names of other jobs that must be completed before this job can start.
    /// </summary>
    public required WorkflowExpressionCollection JobDependencies { get; init; }

    /// <summary>
    ///     Gets the matrix dimensions for running this job in multiple configurations.
    /// </summary>
    public required IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; }
}

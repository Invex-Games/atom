namespace DecSm.Atom.Workflows.Model;

/// <summary>
///     Represents a single step within a workflow job, defining its configuration and behavior.
/// </summary>
/// <param name="Name">The name of the workflow step.</param>
[PublicAPI]
public sealed record WorkflowStepModel(string Name)
{
    // /// <summary>
    // ///     Gets a value indicating whether artifact publishing should be suppressed for this step.
    // /// </summary>
    // /// <remarks>
    // ///     If <c>true</c>, any artifacts produced by this step will not be published. Defaults to <c>false</c>.
    // /// </remarks>
    // public bool SuppressArtifactPublishing { get; init; }

    /// <summary>
    ///     Gets the matrix dimensions for running this step in multiple configurations.
    /// </summary>
    /// <remarks>
    ///     A matrix allows a step to be executed multiple times with different configurations.
    /// </remarks>
    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    /// <summary>
    ///     Gets the options that configure this step's behavior.
    /// </summary>
    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];
}

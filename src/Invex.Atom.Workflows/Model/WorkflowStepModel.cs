namespace Invex.Atom.Workflows.Model;

/// <summary>
///     Represents a single step within a workflow job, defining its configuration and behavior.
/// </summary>
[PublicAPI]
public sealed record WorkflowStepModel
{
    /// <summary>The name of the workflow step.</summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Gets the matrix dimensions for running this step in multiple configurations.
    /// </summary>
    /// <remarks>
    ///     A matrix allows a step to be executed multiple times with different configurations.
    /// </remarks>
    public required IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; }

    /// <summary>
    ///     Gets the options that configure this step's behavior.
    /// </summary>
    public required IReadOnlyList<IBuildOption> Options { get; init; }
}

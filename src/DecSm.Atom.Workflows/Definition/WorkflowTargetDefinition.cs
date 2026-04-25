namespace DecSm.Atom.Workflows.Definition;

/// <summary>
///     Defines a single target or step within a workflow, including its configuration and behavior.
/// </summary>
/// <param name="Name">The name of the workflow target.</param>
[PublicAPI]
public sealed record WorkflowTargetDefinition(string Name)
{
    /// <summary>
    ///     Gets the matrix dimensions for this workflow target, allowing it to run in multiple configurations.
    /// </summary>
    public IReadOnlyList<MatrixDimension> MatrixDimensions { get; init; } = [];

    /// <summary>
    ///     Gets the options that configure this workflow target's behavior.
    /// </summary>
    public IReadOnlyList<IWorkflowOption> Options { get; init; } = [];

    /// <summary>
    ///     Creates a <see cref="WorkflowStepModel" /> from this target definition.
    /// </summary>
    /// <returns>A new <see cref="WorkflowStepModel" /> instance.</returns>
    public WorkflowStepModel CreateModel(IEnumerable<IWorkflowOption> workflowOptions) =>
        new()
        {
            Name = Name,
            MatrixDimensions = MatrixDimensions,
            Options = workflowOptions
                .Concat(Options)
                .ToList(),
        };

    /// <summary>
    ///     Returns a new <see cref="WorkflowTargetDefinition" /> with the specified matrix dimensions added.
    /// </summary>
    /// <param name="dimensions">The matrix dimensions to add.</param>
    /// <returns>A new <see cref="WorkflowTargetDefinition" /> instance.</returns>
    public WorkflowTargetDefinition WithMatrixDimensions(params MatrixDimension[] dimensions) =>
        this with
        {
            MatrixDimensions = MatrixDimensions
                .Concat(dimensions)
                .ToList(),
        };

    /// <summary>
    ///     Returns a new <see cref="WorkflowTargetDefinition" /> with the specified options added.
    /// </summary>
    /// <param name="options">The options to add.</param>
    /// <returns>A new <see cref="WorkflowTargetDefinition" /> instance.</returns>
    public WorkflowTargetDefinition WithOptions(params IEnumerable<IWorkflowOption> options) =>
        this with
        {
            Options = Options
                .Concat(options)
                .ToList(),
        };
}

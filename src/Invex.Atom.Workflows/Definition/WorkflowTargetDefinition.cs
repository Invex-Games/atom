namespace Invex.Atom.Workflows.Definition;

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
    public IReadOnlyList<IBuildOption> Options { get; init; } = [];

    /// <summary>
    ///     Creates a <see cref="WorkflowStepModel" /> from this target definition.
    /// </summary>
    /// <param name="workflowOptions">
    ///     The workflow-level options to merge with this target's own <see cref="Options" />.
    ///     Target-level options take precedence over workflow-level options of the same type.
    /// </param>
    /// <returns>A new <see cref="WorkflowStepModel" /> instance.</returns>
    internal WorkflowStepModel CreateModel(IEnumerable<IBuildOption> workflowOptions) =>
        new()
        {
            Name = Name,
            MatrixDimensions = MergeMatrixDimensions(MatrixDimensions),
            Options = workflowOptions
                .Concat(Options)
                .ToList(),
        };

    /// <summary>
    ///     Returns a new <see cref="WorkflowTargetDefinition" /> with the specified matrix dimensions added.
    /// </summary>
    /// <param name="dimensions">The matrix dimensions to add.</param>
    /// <returns>A new <see cref="WorkflowTargetDefinition" /> instance.</returns>
    public WorkflowTargetDefinition WithMatrixDimensions(params IEnumerable<MatrixDimension> dimensions) =>
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
    public WorkflowTargetDefinition WithOptions(params IEnumerable<IBuildOption> options) =>
        this with
        {
            Options = Options
                .Concat(options)
                .ToList(),
        };

    private static List<MatrixDimension> MergeMatrixDimensions(IEnumerable<MatrixDimension> dimensions) =>
        dimensions
            .GroupBy(x => x.Name)
            .Select(group => group.Count() is 1
                ? group.First()
                : group.First() with
                {
                    Values = group
                        .SelectMany(x => x.Values)
                        .Distinct()
                        .ToArray(),
                })
            .ToList();
}

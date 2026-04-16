namespace DecSm.Atom.Workflows.Writer;

/// <summary>
///     Defines a contract for writers that can generate and validate platform-specific workflow files.
/// </summary>
/// <remarks>
///     This interface is part of a strategy pattern where different writers handle different CI/CD platforms.
///     Implementations are typically registered as singletons and are expected to be thread-safe.
/// </remarks>
[PublicAPI]
public interface IWorkflowWriter
{
    /// <summary>
    ///     Gets the type of workflow that this writer can handle (e.g., GitHub Actions, Azure DevOps).
    /// </summary>
    Type WorkflowType { get; }

    /// <summary>
    ///     Generates a workflow file from the specified workflow model.
    /// </summary>
    /// <param name="workflow">The workflow model to generate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous generation operation.</returns>
    Task Generate(WorkflowModel workflow, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks if the existing workflow file is outdated and needs to be regenerated.
    /// </summary>
    /// <param name="workflow">The workflow model to compare against the existing file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     A task that resolves to <c>true</c> if the workflow file is missing or outdated; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> CheckForDirtyWorkflow(WorkflowModel workflow, CancellationToken cancellationToken = default);
}

/// <summary>
///     Provides a strongly-typed version of <see cref="IWorkflowWriter" /> for a specific workflow type.
/// </summary>
/// <typeparam name="T">The workflow type that this writer handles, which must implement <see cref="IWorkflowType" />.</typeparam>
/// <remarks>
///     This generic interface provides compile-time type safety and automatically resolves the workflow type.
///     Implementations should inherit from this interface rather than <see cref="IWorkflowWriter" /> directly.
/// </remarks>
[PublicAPI]
public interface IWorkflowWriter<T> : IWorkflowWriter
    where T : IWorkflowType
{
    /// <inheritdoc />
    Type IWorkflowWriter.WorkflowType => typeof(T);

    /// <inheritdoc />
    abstract Task IWorkflowWriter.Generate(WorkflowModel workflow, CancellationToken cancellationToken);

    /// <inheritdoc />
    abstract Task<bool> IWorkflowWriter.CheckForDirtyWorkflow(
        WorkflowModel workflow,
        CancellationToken cancellationToken);
}

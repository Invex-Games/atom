namespace DecSm.Atom.Workflows;

/// <summary>
///     Defines a contract for providing workflow options.
/// </summary>
/// <remarks>
///     This interface enables classes to expose a collection of workflow options that can be consumed by
///     workflow engines or configuration systems. Implementations should return a stable, immutable collection.
/// </remarks>
[PublicAPI]
public interface IWorkflowOptionProvider
{
    /// <summary>
    ///     Gets a read-only collection of workflow options provided by this instance.
    /// </summary>
    /// <remarks>
    ///     The collection should never be null, but may be empty. It should be treated as immutable by consumers.
    /// </remarks>
    IReadOnlyList<IWorkflowOption> WorkflowOptions { get; }
}

namespace Invex.Atom.Workflows.WorkflowContext;

/// <summary>
///     Describes the workflow context the build is currently running in, aggregated from all registered
///     <see cref="IWorkflowContextProvider" /> implementations.
/// </summary>
[PublicAPI]
public interface IWorkflowContext
{
    /// <summary>
    ///     Gets the currently running workflow type, or <c>null</c> if no platform was detected.
    /// </summary>
    IWorkflowType? WorkflowType { get; }

    /// <summary>
    ///     Gets the name of the currently running workflow, or <c>null</c> if not available.
    /// </summary>
    string? WorkflowName { get; }
}

internal sealed class WorkflowContext(IEnumerable<IWorkflowContextProvider> providers) : IWorkflowContext
{
    private readonly IWorkflowContextProvider[] _providers = providers.ToArray();

    public IWorkflowType? WorkflowType =>
        field ??= _providers
            .Select(x => x.WorkflowType)
            .OfType<IWorkflowType>()
            .FirstOrDefault();

    public string? WorkflowName =>
        field ??= _providers
            .Select(x => x.WorkflowName)
            .OfType<string>()
            .FirstOrDefault();
}

namespace Invex.Atom.Workflows.WorkflowContext;

[PublicAPI]
public interface IWorkflowContext
{
    IWorkflowType? WorkflowType { get; }

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

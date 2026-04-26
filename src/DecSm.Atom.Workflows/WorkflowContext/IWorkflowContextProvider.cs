namespace DecSm.Atom.Workflows.WorkflowContext;

[PublicAPI]
public interface IWorkflowContextProvider
{
    IWorkflowType? WorkflowType { get; }

    string? WorkflowName { get; }
}

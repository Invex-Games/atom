namespace Invex.Atom.Module.DevOpsWorkflows;

internal sealed class DevOpsWorkflowContextProvider : IWorkflowContextProvider
{
    public IWorkflowType? WorkflowType =>
        DevOps.IsDevOpsPipelines
            ? DevOps.WorkflowType
            : null;

    public string? WorkflowName =>
        DevOps.IsDevOpsPipelines
            ? DevOps.Variables.BuildDefinitionName
            : null;
}

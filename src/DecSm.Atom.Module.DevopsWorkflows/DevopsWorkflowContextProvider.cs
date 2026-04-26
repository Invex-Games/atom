using DecSm.Atom.Workflows.WorkflowContext;

namespace DecSm.Atom.Module.DevopsWorkflows;

internal sealed class DevopsWorkflowContextProvider : IWorkflowContextProvider
{
    public IWorkflowType? WorkflowType =>
        Devops.IsDevopsPipelines
            ? Devops.WorkflowType
            : null;

    public string? WorkflowName =>
        Devops.IsDevopsPipelines
            ? Devops.Variables.BuildDefinitionName
            : null;
}

using DecSm.Atom.Workflows;

namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Devops;

[PublicAPI]
public sealed record DevopsWorkflowType : IWorkflowType
{
    public bool IsRunning => DevopsWorkflows.Devops.IsDevopsPipelines;
}

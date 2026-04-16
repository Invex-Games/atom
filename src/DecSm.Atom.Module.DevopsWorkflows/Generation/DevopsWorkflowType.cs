namespace DecSm.Atom.Module.DevopsWorkflows.Generation;

[PublicAPI]
public sealed record DevopsWorkflowType : IWorkflowType
{
    public bool IsRunning => Devops.IsDevopsPipelines;
}

namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Options;

[PublicAPI]
public record DevopsVariableGroup(string Name) : IWorkflowOption
{
    public bool AllowMultiple => true;
}

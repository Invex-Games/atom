namespace DecSm.Atom.Module.DevopsWorkflows.Generation.Options;

[PublicAPI]
public record DevopsVariableGroup(string Name) : IWorkflowOption
{
    public bool AllowMultiple => true;
}

namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public abstract record ToggleWorkflowOption : IToggleWorkflowOption
{
    public bool Enabled { get; init; } = true;
}

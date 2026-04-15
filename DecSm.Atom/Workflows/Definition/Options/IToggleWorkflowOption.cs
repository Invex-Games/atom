namespace DecSm.Atom.Workflows.Definition.Options;

[PublicAPI]
public interface IToggleWorkflowOption : IWorkflowOption
{
    bool Value { get; }
}

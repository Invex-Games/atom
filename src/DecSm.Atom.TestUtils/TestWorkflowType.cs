namespace DecSm.Atom.TestUtils;

[PublicAPI]
public sealed record TestWorkflowType : IWorkflowType
{
    public bool IsRunning { get; set; } = true;
}

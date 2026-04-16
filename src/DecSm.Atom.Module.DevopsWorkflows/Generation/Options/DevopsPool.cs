namespace DecSm.Atom.Module.DevopsWorkflows.Generation.Options;

[PublicAPI]
public sealed record DevopsPool : IWorkflowOption
{
    public IReadOnlyList<WorkflowExpression> Demands { get; init; } = [];

    public WorkflowExpression? Name { get; init; }

    public WorkflowExpression? HostedImage { get; init; }
}

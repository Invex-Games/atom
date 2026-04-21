namespace DecSm.Atom.Module.DevopsWorkflows.Workflows.Options;

[PublicAPI]
public sealed record DevopsPool : IWorkflowOption
{
    public WorkflowExpressionCollection Demands { get; init; } = [];

    public WorkflowExpression? Name { get; init; }

    public WorkflowExpression? HostedImage { get; init; }
}

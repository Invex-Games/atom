namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Options;

[PublicAPI]
public sealed record GithubRunsOn : IWorkflowOption
{
    public WorkflowExpressionCollection Labels { get; init; } = [];

    public WorkflowExpression? Group { get; init; }
}

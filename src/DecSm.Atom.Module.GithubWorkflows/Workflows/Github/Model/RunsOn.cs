namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record RunsOn
{
    public required WorkflowExpressionCollection Labels { get; init; }

    public WorkflowExpression? Group { get; init; }
}

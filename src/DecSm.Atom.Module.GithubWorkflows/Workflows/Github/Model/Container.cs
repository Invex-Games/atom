namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Container
{
    public required WorkflowExpression Image { get; init; }

    public Credentials? Credentials { get; init; }

    public IReadOnlyDictionary<string, WorkflowExpression>? Env { get; init; }

    public WorkflowExpressionCollection? Ports { get; init; }

    public WorkflowExpressionCollection? Volumes { get; init; }

    public WorkflowExpression? Options { get; init; }
}

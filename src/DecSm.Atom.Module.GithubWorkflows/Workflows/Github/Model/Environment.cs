namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Environment
{
    public required WorkflowExpression Name { get; init; }

    public WorkflowExpression? UrlValue { get; init; }
}

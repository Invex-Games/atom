namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Credentials
{
    public WorkflowExpression? Username { get; init; }

    public WorkflowExpression? Password { get; init; }
}

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Credentials
{
    public WorkflowExpression? Username { get; init; }

    public WorkflowExpression? Password { get; init; }
}

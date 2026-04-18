namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Strategy
{
    public required Matrix Matrix { get; init; }

    public WorkflowExpression? FailFast { get; init; }

    public WorkflowExpression? MaxParallel { get; init; }
}

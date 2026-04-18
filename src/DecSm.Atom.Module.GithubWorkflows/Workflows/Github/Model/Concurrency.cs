namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Concurrency
{
    public required WorkflowExpression Group { get; init; } = "";

    public WorkflowExpression? CancelInProgress { get; init; }
}

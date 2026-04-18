namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Concurrency
{
    public required WorkflowExpression Group { get; init; } = "";

    public WorkflowExpression? CancelInProgress { get; init; }
}

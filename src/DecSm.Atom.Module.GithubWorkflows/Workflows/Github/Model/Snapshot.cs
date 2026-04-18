namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Snapshot
{
    public required WorkflowExpression ImageName { get; init; }

    public WorkflowExpression? Version { get; init; }
}

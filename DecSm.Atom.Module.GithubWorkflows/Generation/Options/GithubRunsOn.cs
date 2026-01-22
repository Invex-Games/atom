namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public sealed record GithubRunsOn : IWorkflowOption
{
    public IReadOnlyList<WorkflowExpression> Labels { get; init; } = [];

    public WorkflowExpression? Group { get; init; }
}

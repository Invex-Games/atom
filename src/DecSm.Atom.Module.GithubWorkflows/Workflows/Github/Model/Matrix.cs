namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Matrix
{
    public IReadOnlyDictionary<string, WorkflowExpressionCollection>? Map { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, WorkflowExpression>>? Include { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, WorkflowExpression>>? Exclude { get; init; }
}

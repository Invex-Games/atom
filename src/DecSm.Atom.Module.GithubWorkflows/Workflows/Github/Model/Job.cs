namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Job
{
    public required WorkflowExpression Name { get; init; }

    public Permissions? Permissions { get; init; }

    public WorkflowExpressionCollection Needs { get; init; } = [];

    public WorkflowExpression? If { get; init; }

    public required RunsOn RunsOn { get; init; }

    public Snapshot? Snapshot { get; init; }

    public Environment? Environment { get; init; }

    public Concurrency? Concurrency { get; init; }

    public IReadOnlyDictionary<string, WorkflowExpression>? Outputs { get; init; }

    public IReadOnlyDictionary<string, WorkflowExpression>? Env { get; init; }

    public WorkflowExpression? TimeoutMinutes { get; init; }

    public Strategy? Strategy { get; init; }

    public WorkflowExpression? ContinueOnError { get; init; }

    public Container? Container { get; init; }

    public IReadOnlyDictionary<string, Container>? Services { get; init; }

    public required IReadOnlyList<Step> Steps { get; init; }
}

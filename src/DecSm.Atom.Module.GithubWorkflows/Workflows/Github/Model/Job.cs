using DecSm.Atom.StructuredText.Expressions;

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Job
{
    public required TextExpression Name { get; init; }

    public Permissions? Permissions { get; init; }

    public TextExpressionCollection Needs { get; init; } = [];

    public TextExpression? If { get; init; }

    public required RunsOn RunsOn { get; init; }

    public Snapshot? Snapshot { get; init; }

    public Environment? Environment { get; init; }

    public Concurrency? Concurrency { get; init; }

    public IReadOnlyDictionary<string, TextExpression>? Outputs { get; init; }

    public IReadOnlyDictionary<string, TextExpression>? Env { get; init; }

    public TextExpression? TimeoutMinutes { get; init; }

    public Strategy? Strategy { get; init; }

    public TextExpression? ContinueOnError { get; init; }

    public Container? Container { get; init; }

    public IReadOnlyDictionary<string, Container>? Services { get; init; }

    public required IReadOnlyList<Step> Steps { get; init; }
}

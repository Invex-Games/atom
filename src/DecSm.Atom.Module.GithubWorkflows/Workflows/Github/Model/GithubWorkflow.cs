using DecSm.Atom.StructuredText.Expressions;

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record GithubWorkflow
{
    public string? Name { get; init; }

    public TextExpression? RunName { get; init; }

    public required IReadOnlyList<On> On { get; init; }

    public Permissions? Permissions { get; init; }

    public IReadOnlyDictionary<string, TextExpression>? Env { get; init; }

    public Concurrency? Concurrency { get; init; }

    public required IReadOnlyList<Job> Jobs { get; init; }
}

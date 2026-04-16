namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Job
{
    public required string Name { get; init; }

    public Permissions? Permissions { get; init; }

    public IReadOnlyList<string> Needs { get; init; } = [];

    public string? If { get; init; }

    public required RunsOn RunsOn { get; init; }

    public Snapshot? Snapshot { get; init; }

    public Environment? Environment { get; init; }

    public Concurrency? Concurrency { get; init; }

    public IReadOnlyDictionary<string, string>? Outputs { get; init; }

    public IReadOnlyDictionary<string, string>? Env { get; init; }

    public string? TimeoutMinutes { get; init; }

    public Strategy? Strategy { get; init; }

    public string? ContinueOnError { get; init; }

    public Container? Container { get; init; }

    public IReadOnlyDictionary<string, Container>? Services { get; init; }

    public required IReadOnlyList<Step> Steps { get; init; }
}

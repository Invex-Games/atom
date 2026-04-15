namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record GithubWorkflow
{
    public string? Name { get; init; }

    public string? RunName { get; init; }

    public required IReadOnlyList<On> On { get; init; }

    public Permissions? Permissions { get; init; }

    public IReadOnlyDictionary<string, string>? Env { get; init; }

    public Concurrency? Concurrency { get; init; }

    public required IReadOnlyList<Job> Jobs { get; init; }
}

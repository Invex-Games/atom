namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record RunsOn
{
    public required IReadOnlyList<string> Labels { get; init; }

    public string? Group { get; init; }
}

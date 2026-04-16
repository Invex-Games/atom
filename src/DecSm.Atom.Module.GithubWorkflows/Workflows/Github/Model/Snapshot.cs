namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Snapshot
{
    public required string ImageName { get; init; }

    public string? Version { get; init; }
}

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Concurrency
{
    public required string Group { get; init; } = "";

    public string? CancelInProgress { get; init; }
}

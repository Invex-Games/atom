namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Strategy
{
    public required Matrix Matrix { get; init; }

    public string? FailFast { get; init; }

    public string? MaxParallel { get; init; }
}

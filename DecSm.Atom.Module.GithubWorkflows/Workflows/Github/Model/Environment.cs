namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Environment
{
    public required string Name { get; init; }

    public string? UrlValue { get; init; }
}

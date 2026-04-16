namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Container
{
    public required string Image { get; init; }

    public Credentials? Credentials { get; init; }

    public IReadOnlyDictionary<string, string>? Env { get; init; }

    public IReadOnlyList<string>? Ports { get; init; }

    public IReadOnlyList<string>? Volumes { get; init; }

    public string? Options { get; init; }
}

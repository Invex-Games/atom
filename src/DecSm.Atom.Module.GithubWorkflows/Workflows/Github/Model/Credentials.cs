namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[UnstableAPI]
public sealed record Credentials
{
    public string? Username { get; init; }

    public string? Password { get; init; }
}

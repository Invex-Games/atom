namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Snapshot
{
    public required TextExpression ImageName { get; init; }

    public TextExpression? Version { get; init; }
}

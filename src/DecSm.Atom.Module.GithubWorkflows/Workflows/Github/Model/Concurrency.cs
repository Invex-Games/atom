namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Concurrency
{
    public required TextExpression Group { get; init; } = "";

    public TextExpression? CancelInProgress { get; init; }
}

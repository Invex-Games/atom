namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Environment
{
    public required TextExpression Name { get; init; }

    public TextExpression? UrlValue { get; init; }
}

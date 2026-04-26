namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Credentials
{
    public TextExpression? Username { get; init; }

    public TextExpression? Password { get; init; }
}

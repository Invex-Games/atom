namespace Invex.Atom.Module.GithubWorkflows.Options;

[PublicAPI]
public sealed record GithubRunsOn : IBuildOption
{
    public TextExpressionCollection Labels { get; init; } = [];

    public TextExpression? Group { get; init; }
}

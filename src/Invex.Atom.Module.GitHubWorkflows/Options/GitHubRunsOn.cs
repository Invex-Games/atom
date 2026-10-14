namespace Invex.Atom.Module.GitHubWorkflows.Options;

[PublicAPI]
public sealed record GitHubRunsOn : IBuildOption
{
    public TextExpressionCollection Labels { get; init; } = [];

    public TextExpression? Group { get; init; }
}

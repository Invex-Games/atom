namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Container
{
    public required TextExpression Image { get; init; }

    public Credentials? Credentials { get; init; }

    public IReadOnlyDictionary<string, TextExpression>? Env { get; init; }

    public TextExpressionCollection? Ports { get; init; }

    public TextExpressionCollection? Volumes { get; init; }

    public TextExpression? Options { get; init; }
}

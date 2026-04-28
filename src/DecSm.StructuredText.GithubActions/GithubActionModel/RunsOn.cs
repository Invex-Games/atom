namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record RunsOn
{
    public required TextExpressionCollection Labels { get; init; }

    public TextExpression? Group { get; init; }
}

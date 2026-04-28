namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Environment
{
    public required TextExpression Name { get; init; }

    public TextExpression? UrlValue { get; init; }
}

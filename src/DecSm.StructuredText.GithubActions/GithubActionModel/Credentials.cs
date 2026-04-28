namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Credentials
{
    public TextExpression? Username { get; init; }

    public TextExpression? Password { get; init; }
}

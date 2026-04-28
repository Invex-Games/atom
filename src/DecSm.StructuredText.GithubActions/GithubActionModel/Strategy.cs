namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Strategy
{
    public required Matrix Matrix { get; init; }

    public TextExpression? FailFast { get; init; }

    public TextExpression? MaxParallel { get; init; }
}

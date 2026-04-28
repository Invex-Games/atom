namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Concurrency
{
    public required TextExpression Group { get; init; } = "";

    public TextExpression? CancelInProgress { get; init; }
}

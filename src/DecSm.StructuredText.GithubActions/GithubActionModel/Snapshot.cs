namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Snapshot
{
    public required TextExpression ImageName { get; init; }

    public TextExpression? Version { get; init; }
}

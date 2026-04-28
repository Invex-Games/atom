namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
public sealed record Matrix
{
    public IReadOnlyDictionary<string, TextExpressionCollection>? Map { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, TextExpression>>? Include { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, TextExpression>>? Exclude { get; init; }
}

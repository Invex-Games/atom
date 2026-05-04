namespace DecSm.StructuredText.GithubActions.GithubActionModel;

[PublicAPI]
[Union]
public partial record Step
{
    public string? Id { get; init; }

    public TextExpression? If { get; init; }

    public TextExpression? Name { get; init; }

    public TextExpression? WorkingDirectory { get; init; }

    public IReadOnlyDictionary<string, TextExpressionCollection>? With { get; init; }

    public IReadOnlyDictionary<string, TextExpression>? Env { get; init; }

    public TextExpression? ContinueOnError { get; init; }

    public TextExpression? TimeoutMinutes { get; init; }

    [PublicAPI]
    public partial record UsesStep
    {
        public required TextExpression Uses { get; init; }
    }

    [PublicAPI]
    public partial record RunStep
    {
        public required TextExpressionCollection Run { get; init; }

        public TextExpression? Shell { get; init; }
    }
}

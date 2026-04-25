using DecSm.Atom.StructuredText.Expressions;

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record Matrix
{
    public IReadOnlyDictionary<string, TextExpressionCollection>? Map { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, TextExpression>>? Include { get; init; }

    public IReadOnlyList<IReadOnlyDictionary<string, TextExpression>>? Exclude { get; init; }
}

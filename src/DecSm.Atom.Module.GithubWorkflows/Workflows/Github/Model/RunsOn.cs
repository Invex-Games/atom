using DecSm.Atom.StructuredText.Expressions;

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github.Model;

[PublicAPI]
public sealed record RunsOn
{
    public required TextExpressionCollection Labels { get; init; }

    public TextExpression? Group { get; init; }
}

using DecSm.Atom.StructuredText.Expressions;

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Options;

[PublicAPI]
public sealed record GithubRunsOn : IWorkflowOption
{
    public TextExpressionCollection Labels { get; init; } = [];

    public TextExpression? Group { get; init; }
}

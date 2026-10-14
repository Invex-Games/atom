namespace Invex.Atom.Module.DevOpsWorkflows.Options;

[PublicAPI]
public sealed record DevOpsPool : IBuildOption
{
    public TextExpressionCollection Demands { get; init; } = [];

    public TextExpression? Name { get; init; }

    public TextExpression? HostedImage { get; init; }
}

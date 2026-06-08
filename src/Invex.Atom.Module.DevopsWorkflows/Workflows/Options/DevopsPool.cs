namespace Invex.Atom.Module.DevopsWorkflows.Workflows.Options;

[PublicAPI]
public sealed record DevopsPool : IBuildOption
{
    public TextExpressionCollection Demands { get; init; } = [];

    public TextExpression? Name { get; init; }

    public TextExpression? HostedImage { get; init; }
}

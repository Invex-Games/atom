namespace Invex.Atom.Module.DevOpsWorkflows.Options;

[PublicAPI]
public sealed record DevOpsPool : IBuildOption
{
    public IReadOnlyList<Template> Demands { get; init; } = [];

    public Template? Name { get; init; }

    public Template? HostedImage { get; init; }
}

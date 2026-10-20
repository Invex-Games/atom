namespace Invex.Atom.Module.GitHubWorkflows.Options;

[PublicAPI]
public sealed record GitHubRunsOn : IBuildOption
{
    public IReadOnlyList<Template> Labels { get; init; } = [];

    public Template? Group { get; init; }
}

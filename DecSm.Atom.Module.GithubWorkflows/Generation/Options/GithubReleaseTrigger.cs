namespace DecSm.Atom.Module.GithubWorkflows.Generation.Options;

[PublicAPI]
public sealed record GithubReleaseTrigger : IWorkflowTrigger
{
    public IReadOnlyList<string> Types { get; init; } = [];
}

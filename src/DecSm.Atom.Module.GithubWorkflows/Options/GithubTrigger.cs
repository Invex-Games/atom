namespace DecSm.Atom.Module.GithubWorkflows.Options;

[PublicAPI]
public sealed record GithubTrigger(On On) : IWorkflowTrigger;

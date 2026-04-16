namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Options;

[PublicAPI]
public sealed record GithubTrigger(On On) : IWorkflowTrigger;

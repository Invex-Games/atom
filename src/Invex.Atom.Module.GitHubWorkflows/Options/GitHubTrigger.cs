namespace Invex.Atom.Module.GitHubWorkflows.Options;

[PublicAPI]
public sealed record GitHubTrigger(On On) : IWorkflowTrigger;

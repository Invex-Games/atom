namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Options;

[PublicAPI]
public sealed record GithubTokenPermissionsOption(Permissions Permissions) : IWorkflowOption;

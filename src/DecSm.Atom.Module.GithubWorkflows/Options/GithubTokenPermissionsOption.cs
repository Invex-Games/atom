namespace DecSm.Atom.Module.GithubWorkflows.Options;

[PublicAPI]
public sealed record GithubTokenPermissionsOption(Permissions Permissions) : IBuildOption;

namespace Invex.Atom.Module.GitHubWorkflows.Options;

[PublicAPI]
public sealed record GitHubTokenPermissionsOption(Permissions Permissions) : IBuildOption;

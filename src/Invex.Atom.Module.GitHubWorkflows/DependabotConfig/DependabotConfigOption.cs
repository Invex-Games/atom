namespace Invex.Atom.Module.GitHubWorkflows.DependabotConfig;

[PublicAPI]
public sealed record DependabotConfigOption(StructuredText.GithubActions.DependabotConfigModel.Model.DependabotConfig Config)
    : IBuildOption;

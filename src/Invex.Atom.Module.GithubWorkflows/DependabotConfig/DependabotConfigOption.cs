namespace Invex.Atom.Module.GithubWorkflows.DependabotConfig;

[PublicAPI]
public sealed record DependabotConfigOption(StructuredText.GithubActions.DependabotConfigModel.Model.DependabotConfig Config)
    : IBuildOption;

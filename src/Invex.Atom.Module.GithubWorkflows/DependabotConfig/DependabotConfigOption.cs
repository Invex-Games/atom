namespace Invex.Atom.Module.GithubWorkflows.DependabotConfig;

[PublicAPI]
public record DependabotConfigOption(StructuredText.GithubActions.DependabotConfigModel.Model.DependabotConfig Config)
    : IBuildOption;

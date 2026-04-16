namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot;

[PublicAPI]
public record DependabotConfigOption(DependabotConfig Config) : IWorkflowOption;

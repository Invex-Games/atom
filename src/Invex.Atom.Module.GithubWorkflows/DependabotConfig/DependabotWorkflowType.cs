namespace Invex.Atom.Module.GithubWorkflows.DependabotConfig;

[PublicAPI]
public sealed record DependabotWorkflowType : IWorkflowType
{
    public bool IsRunning => !string.IsNullOrWhiteSpace(Github.Variables.Workflow);
}

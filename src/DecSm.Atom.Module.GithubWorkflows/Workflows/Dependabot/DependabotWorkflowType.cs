using DecSm.Atom.Workflows;

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Dependabot;

[PublicAPI]
public sealed record DependabotWorkflowType : IWorkflowType
{
    public bool IsRunning => !string.IsNullOrWhiteSpace(GithubWorkflows.Github.Variables.Workflow);
}

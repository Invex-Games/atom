namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Github;

[PublicAPI]
public sealed record GithubWorkflowType : IWorkflowType
{
    public bool IsRunning => GithubWorkflows.Github.IsGithubActions;
}

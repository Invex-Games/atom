namespace DecSm.Atom.Module.GithubWorkflows.GithubActions;

[PublicAPI]
public sealed record GithubWorkflowType : IWorkflowType
{
    public bool IsRunning => Github.IsGithubActions;
}

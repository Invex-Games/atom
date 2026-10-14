namespace Invex.Atom.Module.GitHubWorkflows;

internal sealed class GitHubWorkflowContextProvider : IWorkflowContextProvider
{
    public IWorkflowType? WorkflowType =>
        GitHub.IsGitHubActions
            ? WorkflowTypes.GitHub.Action
            : null;

    public string? WorkflowName =>
        GitHub.IsGitHubActions
            ? GitHub.Variables.Workflow
            : null;
}

namespace Invex.Atom.Module.GithubWorkflows;

internal sealed class GithubWorkflowContextProvider : IWorkflowContextProvider
{
    public IWorkflowType? WorkflowType =>
        Github.IsGithubActions
            ? WorkflowTypes.Github.Action
            : null;

    public string? WorkflowName =>
        Github.IsGithubActions
            ? Github.Variables.Workflow
            : null;
}

namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Steps;

[PublicAPI]
public interface IGithubAdditionalStepOption : IAdditionalStepOption
{
    Step Build(IWorkflowExpressionResolver expressionResolver);
}

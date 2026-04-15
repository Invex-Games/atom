namespace DecSm.Atom.Module.GithubWorkflows.Workflows.Steps;

[PublicAPI]
public sealed record GithubCustomAdditionalStep : IGithubAdditionalStepOption
{
    public required int Order { get; init; }

    public required Step Step { get; init; }

    public Step Build(IWorkflowExpressionResolver expressionResolver) =>
        Step;
}

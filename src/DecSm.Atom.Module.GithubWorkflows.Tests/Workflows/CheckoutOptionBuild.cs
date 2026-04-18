namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class CheckoutOptionBuild : MinimalBuildDefinition, IGithubWorkflows, ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("checkoutoption-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets =
            [
                WorkflowTargets.CheckoutOptionTarget1,
                WorkflowTargets.CheckoutOptionTarget2.WithOptions(WorkflowOptions.Github.Steps.Checkout(new()
                {
                    Submodules = WorkflowExpressions.From("recursive"),
                    Token = WorkflowExpressions.From("some-token"),
                    FetchDepth = 0,
                    Lfs = true,
                })),
                WorkflowTargets.CheckoutOptionTarget3.WithOptions(WorkflowOptions.Github.Steps.Checkout(new()
                {
                    Value = false,
                })),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

public interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget1 => t => t;

    Target CheckoutOptionTarget2 => t => t;

    Target CheckoutOptionTarget3 => t => t;
}

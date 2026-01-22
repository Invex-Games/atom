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
                WorkflowTargets.CheckoutOptionTarget.WithOptions(
                    WorkflowOptions.Github.ConfigureCheckout("v4", true, "recursive", "some-token")),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

public interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget => t => t;
}

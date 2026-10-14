namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class CheckoutOptionBuild : WorkflowBuildDefinition,
    IWorkflowBuildDefinition,
    IDevOpsWorkflows,
    ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("checkoutoption-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets =
            [
                new(nameof(ICheckoutOptionTarget.CheckoutOptionTarget))
                {
                    Options =
                    [
                        BuildOptions.DevOps.Steps.Checkout(new()
                        {
                            DisplayName = "Checkout with no options",
                        }),
                    ],
                },
            ],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];
}

public interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget => t => t;
}

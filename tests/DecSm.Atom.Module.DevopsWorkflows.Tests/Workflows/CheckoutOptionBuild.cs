namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class CheckoutOptionBuild : WorkflowBuildDefinition, IDevopsWorkflows, ICheckoutOptionTarget
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
                        BuildOptions.Devops.Steps.Checkout(new()
                        {
                            DisplayName = "Checkout with no options",
                        }),
                    ],
                },
            ],
            Types = [WorkflowTypes.Devops.Pipeline],
        },
    ];
}

public interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget => t => t;
}

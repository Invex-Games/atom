namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class CheckoutOptionBuild : WorkflowBuildDefinition, IGitHubWorkflows, ICheckoutOptionTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("checkoutoption-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets =
            [
                new(nameof(ICheckoutOptionTarget.CheckoutOptionTarget1)),
                new(nameof(ICheckoutOptionTarget.CheckoutOptionTarget2))
                {
                    Options =
                    [
                        BuildOptions.GitHub.Steps.Checkout(new()
                        {
                            Submodules = "recursive",
                            Token = "some-token",
                            FetchDepth = 0,
                            Lfs = true,
                        }),
                    ],
                },
                new(nameof(ICheckoutOptionTarget.CheckoutOptionTarget3))
                {
                    Options =
                    [
                        BuildOptions.GitHub.Steps.Checkout(new()
                        {
                            Enabled = false,
                        }),
                    ],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface ICheckoutOptionTarget
{
    Target CheckoutOptionTarget1 => t => t;

    Target CheckoutOptionTarget2 => t => t;

    Target CheckoutOptionTarget3 => t => t;
}

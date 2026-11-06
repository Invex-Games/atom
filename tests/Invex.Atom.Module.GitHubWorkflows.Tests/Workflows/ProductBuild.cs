namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ProductBuild : WorkflowBuildDefinition, IGitHubWorkflows, IProductTargets
{
    IReadOnlyList<Product> IProductsDefinition.Products =>
    [
        new("Shared")
        {
            Paths = ["src/Shared/"],
        },
        new("AppA")
        {
            Paths = ["src/AppA/**"],
            DependsOn = ["Shared"],
            GitVersionConfig = "src/AppA/GitVersion.yml",
        },
        new("AppB")
        {
            Paths = ["src/AppB/**"],
            DependsOn = ["Shared"],
        },
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        WorkflowPresets.ProductValidate(this,
            nameof(IProductTargets.ValidateGate),
            [
                new("AppA", nameof(IProductTargets.TestAppA)),
                new("AppB", nameof(IProductTargets.TestAppB)),
            ]) with
        {
            Types = [WorkflowTypes.GitHub.Action],
        },
        WorkflowDefinition
            .ForProduct(this, "AppA", "Build")
            .WithTriggers(WorkflowTriggers.Manual, WorkflowTriggers.GitHub.OnRelease()) with
            {
                Targets = [new(nameof(IProductTargets.PackAppA))],
                Types = [WorkflowTypes.GitHub.Action],
            },
        WorkflowDefinition.ForProduct(this, "AppB", "Build") with
        {
            Targets = [new(nameof(IProductTargets.PackAppB))],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface IProductTargets : IProductsDefinition
{
    Target TestAppA =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts));

    Target TestAppB =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts));

    Target PackAppA => t => t;

    Target PackAppB => t => t;

    Target ValidateGate =>
        t => t
            .DependsOn(nameof(TestAppA))
            .DependsOn(nameof(TestAppB));
}

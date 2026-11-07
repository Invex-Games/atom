namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ProductBuild : WorkflowBuildDefinition, IDevOpsWorkflows, IProductTargets
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
            GitVersionConfig = FileSystem.AtomRootDirectory / "src/AppA/GitVersion.yml",
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
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
        WorkflowDefinition.ForProduct(this, "AppA", "Build", releaseTagTrigger: true) with
        {
            Targets = [new(nameof(IProductTargets.PackAppA))],
            Types = [WorkflowTypes.DevOps.Pipeline],
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

    Target ValidateGate =>
        t => t
            .DependsOn(nameof(TestAppA))
            .DependsOn(nameof(TestAppB));
}

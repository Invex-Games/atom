namespace Invex.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class ProductWorkflowBuild : WorkflowBuildDefinition, IProductWorkflowTargets
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
            TagPrefix = "app-a@",
        },
    ];

    public override IReadOnlyList<WorkflowDefinition> Workflows => [];
}

public interface IProductWorkflowTargets : IProductsDefinition
{
    Target TestAppA =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts));

    Target UnwiredAppA => t => t;

    Target BuildShared =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts));

    Target TestAppAOnShared =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts))
            .DependsOn(nameof(BuildShared));

    Target PackAppA => t => t;

    Target TestAppAOnPack =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts))
            .DependsOn(nameof(PackAppA));

    Target LintAppA =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts))
            .DependsOn(nameof(TestAppA));

    Target Compile => t => t;

    Target TestAppAOnCompile =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts))
            .DependsOn(nameof(Compile));

    Target CompileGate =>
        t => t
            .DependsOn(nameof(TestAppAOnCompile))
            .DependsOn(nameof(Compile));

    Target LintGate =>
        t => t
            .DependsOn(nameof(TestAppA))
            .DependsOn(nameof(LintAppA));

    Target Gate =>
        t => t
            .DependsOn(nameof(TestAppA));

    Target CompileAfterTestAppA =>
        t => t
            .DependsOn(nameof(TestAppA));

    Target CompileAfterPack =>
        t => t
            .DependsOn(nameof(PackAppA));

    Target LintAll => t => t;

    Target LintAllGate =>
        t => t
            .DependsOn(nameof(TestAppA))
            .DependsOn(nameof(LintAll));
}

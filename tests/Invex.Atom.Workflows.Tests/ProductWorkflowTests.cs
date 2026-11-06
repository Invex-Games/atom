namespace Invex.Atom.Workflows.Tests;

[TestFixture]
internal sealed class ProductWorkflowTests
{
    private static (IHost Host, ProductWorkflowBuild Build) CreateHost()
    {
        var host = CreateWorkflowTestHost<ProductWorkflowBuild>();

        return (host, (ProductWorkflowBuild)host.Services.GetRequiredService<IBuildDefinition>());
    }

    [Test]
    public void ForProduct_CreatesSinglePushTriggerAndProductOptions()
    {
        var (_, build) = CreateHost();

        var definition = WorkflowDefinition.ForProduct(build, "appa", "Build", ["main", "release/*"], true);

        definition.Name.ShouldBe("AppA - Build");

        definition
            .Triggers
            .ShouldHaveSingleItem()
            .ShouldBeOfType<GitPushTrigger>()
            .ShouldSatisfyAllConditions(t => t.IncludedBranches.ShouldBe(["main", "release/*"]),
                t => t.IncludedPaths.ShouldBe(["src/AppA/**", "src/Shared/**", .. IProductsDefinition.DefaultGlobalPaths]),
                t => t.IncludedTags.ShouldBe(["app-a@*"]));

        definition
            .Options
            .ShouldBe([
                new WorkflowParamInjection(nameof(IProductsDefinition.ProductName), "AppA"),
                new ProductWorkflowOption("AppA", "app-a@"),
            ]);
    }

    [Test]
    public void ForProduct_WithoutReleaseTagTrigger_HasNoTags()
    {
        var (_, build) = CreateHost();

        WorkflowDefinition
            .ForProduct(build, "AppA", "Build")
            .WithTriggers(WorkflowTriggers.Manual)
            .Triggers
            .ShouldSatisfyAllConditions(triggers => triggers.Count.ShouldBe(2),
                triggers => triggers[0]
                    .ShouldBeOfType<GitPushTrigger>()
                    .ShouldSatisfyAllConditions(t => t.IncludedBranches.ShouldBe(["main"]),
                        t => t.IncludedTags.ShouldBeEmpty()));
    }

    [Test]
    public void ForProduct_WithUnknownProduct_Throws()
    {
        var (_, build) = CreateHost();

        Should.Throw<InvalidOperationException>(() => WorkflowDefinition.ForProduct(build, "AppZ", "Build"));
    }

    [Test]
    public void ProductValidate_WiresDetectionProductTargetsAndGate()
    {
        var (_, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("appa", nameof(IProductWorkflowTargets.TestAppA))]);

        definition.Name.ShouldBe("Validate");

        definition
            .Targets
            .Select(x => x.Name)
            .ShouldBe([
                nameof(IProductsDefinition.DetectAffectedProducts), nameof(IProductWorkflowTargets.TestAppA),
                nameof(IProductWorkflowTargets.Gate),
            ]);

        definition
            .Targets[1]
            .Options
            .ShouldSatisfyAllConditions(o => o
                    .OfType<WorkflowParamInjection>()
                    .ShouldHaveSingleItem()
                    .ShouldBe(new(nameof(IProductsDefinition.ProductName), "AppA")),
                o => o
                    .OfType<TargetCondition>()
                    .ShouldHaveSingleItem()
                    .TargetNames
                    .ShouldBe([nameof(IProductsDefinition.DetectAffectedProducts)]));

        definition
            .Options
            .ShouldHaveSingleItem()
            .ShouldBeOfType<ProductValidateOption>()
            .ShouldSatisfyAllConditions(o => o.GateTarget.ShouldBe(nameof(IProductWorkflowTargets.Gate)),
                o => o
                    .ProductTargets
                    .ShouldHaveSingleItem()
                    .ShouldSatisfyAllConditions(x => x.Product.ShouldBe("AppA"),
                        x => x.Target.Name.ShouldBe(nameof(IProductWorkflowTargets.TestAppA))));
    }

    [Test]
    public void ProductValidate_WithTargetListedTwice_Throws()
    {
        var (_, build) = CreateHost();

        Should
            .Throw<ArgumentException>(() => WorkflowPresets.ProductValidate(build,
                nameof(IProductWorkflowTargets.Gate),
                [
                    new("AppA", nameof(IProductWorkflowTargets.TestAppA)),
                    new("Shared", nameof(IProductWorkflowTargets.TestAppA)),
                ]))
            .Message
            .ShouldStartWith(
                "Target 'TestAppA' is listed more than once. Each product needs its own targets, because a job runs with one product; a target that does not depend on the product can be a shared target.");
    }

    [TestCase(nameof(IProductWorkflowTargets.Gate))]
    [TestCase(nameof(IProductsDefinition.DetectAffectedProducts))]
    public void ProductValidate_WithReservedTarget_Throws(string target)
    {
        var (_, build) = CreateHost();

        Should
            .Throw<ArgumentException>(() => WorkflowPresets.ProductValidate(build,
                nameof(IProductWorkflowTargets.Gate),
                [new("AppA", nameof(IProductWorkflowTargets.TestAppA)), new("AppA", target)]))
            .Message
            .ShouldStartWith($"'{target}' cannot be a product or shared target: the workflow adds it itself.");
    }

    [Test]
    public void Resolve_WithDependencyOnAnotherProductsTarget_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [
                new("Shared", nameof(IProductWorkflowTargets.BuildShared)),
                new("AppA", nameof(IProductWorkflowTargets.TestAppAOnShared)),
            ]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldContain(
                "Product target 'TestAppAOnShared' of product 'AppA' depends on product target 'BuildShared' of product 'Shared', so it is skipped whenever 'AppA' is affected but 'Shared' is not, and the gate passes. Give each product its own targets.");
    }

    [Test]
    public void Resolve_WithDependencyOnUnlistedTarget_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppAOnPack))]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldContain(
                "Product target 'TestAppAOnPack' depends on 'PackAppA', which is not a product or shared target of the workflow, so it would be added as a job without options or dependencies. List it as a product target of 'AppA' if it builds the product, as a shared target if it does not depend on the product, or remove the dependency.");
    }

    [Test]
    public void Resolve_WithSharedTarget_RunsItUnconditionally()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.CompileGate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppAOnCompile))],
            sharedTargets: [new(nameof(IProductWorkflowTargets.Compile))]);

        var jobs = host
            .Services
            .GetRequiredService<WorkflowResolver>()
            .Resolve(definition)
            .Jobs;

        jobs
            .Select(x => x.Name)
            .ShouldBe([
                nameof(IProductsDefinition.DetectAffectedProducts), nameof(IProductWorkflowTargets.Compile),
                nameof(IProductWorkflowTargets.TestAppAOnCompile), nameof(IProductWorkflowTargets.CompileGate),
            ]);

        jobs
            .Single(x => x.Name is nameof(IProductWorkflowTargets.Compile))
            .TargetStep
            .Options
            .ShouldSatisfyAllConditions(o => o
                    .OfType<WorkflowParamInjection>()
                    .ShouldBeEmpty(),
                o => o
                    .OfType<TargetCondition>()
                    .ShouldBeEmpty());
    }

    [Test]
    public void Resolve_WithSharedTargetDependingOnProductTarget_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))],
            sharedTargets: [new(nameof(IProductWorkflowTargets.CompileAfterTestAppA))]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldContain(
                "Shared target 'CompileAfterTestAppA' depends on product target 'TestAppA' of product 'AppA', so it is skipped whenever 'AppA' is not affected, which skips the product targets that depend on it, and the gate passes. A shared target can depend only on other shared targets and 'DetectAffectedProducts'.");
    }

    [Test]
    public void Resolve_WithSharedTargetDependingOnUnlistedTarget_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))],
            sharedTargets: [new(nameof(IProductWorkflowTargets.CompileAfterPack))]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldContain(
                "Shared target 'CompileAfterPack' depends on 'PackAppA', which is not a shared target of the workflow, so it would be added as a job without options or dependencies. List it as a shared target, or remove the dependency.");
    }

    [Test]
    public void Resolve_WithSharedTargetOutsideTheGate_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))],
            sharedTargets: [new(nameof(IProductWorkflowTargets.LintAll))]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldBe(
                "Workflow 'Validate' is not wired correctly: Gate target 'Gate' must depend on shared target 'LintAll', directly or through a product target, or the gate does not fail when that job fails.");
    }

    [Test]
    public void Resolve_WithIndependentSharedTargetInTheGate_Succeeds()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.LintAllGate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))],
            sharedTargets: [new(nameof(IProductWorkflowTargets.LintAll))]);

        host
            .Services
            .GetRequiredService<WorkflowResolver>()
            .Resolve(definition)
            .Jobs
            .Select(x => x.Name)
            .ShouldBe([
                nameof(IProductsDefinition.DetectAffectedProducts), nameof(IProductWorkflowTargets.LintAll),
                nameof(IProductWorkflowTargets.TestAppA), nameof(IProductWorkflowTargets.LintAllGate),
            ]);
    }

    [Test]
    public void ProductValidate_WithSharedTargetAlsoAProductTarget_Throws()
    {
        var (_, build) = CreateHost();

        Should.Throw<ArgumentException>(() => WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))],
            sharedTargets: [new(nameof(IProductWorkflowTargets.TestAppA))]));
    }

    [Test]
    public void Resolve_WithGateDependingOnUnlistedTarget_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.CompileGate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppAOnCompile))]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldContain(
                "Gate target 'CompileGate' depends on 'Compile', which is not a product or shared target of the workflow, so it would be added as a job without options or dependencies.");
    }

    [Test]
    public void Resolve_HandBuiltWithOptionAndMissingTarget_Throws()
    {
        var (host, _) = CreateHost();

        var definition = new WorkflowDefinition("Validate")
        {
            Options =
            [
                new ProductValidateOption(nameof(IProductWorkflowTargets.LintGate),
                    [
                        new("AppA", nameof(IProductWorkflowTargets.TestAppA)),
                        new("AppA", nameof(IProductWorkflowTargets.LintAppA)),
                    ]),
            ],
            Targets =
            [
                new(nameof(IProductsDefinition.DetectAffectedProducts)),
                new(nameof(IProductWorkflowTargets.TestAppA)),
                new(nameof(IProductWorkflowTargets.LintGate)),
            ],
        };

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldBe(
                "Workflow 'Validate' is not wired correctly: Target 'LintAppA' is not one of the workflow's targets, so it would be added as a job without its options. Add it to the workflow's targets.");
    }

    [Test]
    public void Resolve_WithDependencyOnSameProductsTarget_Succeeds()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.LintGate),
            [
                new("AppA", nameof(IProductWorkflowTargets.TestAppA)),
                new("appa", nameof(IProductWorkflowTargets.LintAppA)),
            ]);

        host
            .Services
            .GetRequiredService<WorkflowResolver>()
            .Resolve(definition)
            .Jobs
            .Select(x => x.Name)
            .ShouldBe([
                nameof(IProductsDefinition.DetectAffectedProducts), nameof(IProductWorkflowTargets.TestAppA),
                nameof(IProductWorkflowTargets.LintAppA), nameof(IProductWorkflowTargets.LintGate),
            ]);
    }

    [Test]
    public void Resolve_WithCorrectWiring_Succeeds()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))]);

        host
            .Services
            .GetRequiredService<WorkflowResolver>()
            .Resolve(definition)
            .Jobs
            .Select(x => x.Name)
            .ShouldBe([
                nameof(IProductsDefinition.DetectAffectedProducts), nameof(IProductWorkflowTargets.TestAppA),
                nameof(IProductWorkflowTargets.Gate),
            ]);
    }

    [Test]
    public void Resolve_WithMissingDependencies_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            nameof(IProductWorkflowTargets.Gate),
            [
                new("AppA", nameof(IProductWorkflowTargets.TestAppA)),
                new("AppA", nameof(IProductWorkflowTargets.UnwiredAppA)),
            ]);

        Should
            .Throw<InvalidOperationException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldBe(
                "Workflow 'Validate' is not wired correctly: Product target 'UnwiredAppA' must depend directly on 'DetectAffectedProducts' (.DependsOn(nameof(IProductsDefinition.DetectAffectedProducts))), or its job cannot read the affected products and is always skipped. Gate target 'Gate' must depend directly on product target 'UnwiredAppA', or the gate does not fail when that job fails.");
    }

    [Test]
    public void Resolve_WithUnknownGate_Throws()
    {
        var (host, build) = CreateHost();

        var definition = WorkflowPresets.ProductValidate(build,
            "MissingGate",
            [new("AppA", nameof(IProductWorkflowTargets.TestAppA))]);

        Should
            .Throw<ArgumentException>(() => host
                .Services
                .GetRequiredService<WorkflowResolver>()
                .Resolve(definition))
            .Message
            .ShouldBe("Target 'MissingGate' not found.");
    }
}

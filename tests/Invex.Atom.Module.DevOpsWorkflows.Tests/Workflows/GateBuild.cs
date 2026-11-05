namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class GateBuild : WorkflowBuildDefinition, IDevOpsWorkflows, IGateTargets
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        // Product jobs run only when their product is affected; the gate runs (and fails) only if one failed
        new("validate-gate-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                new(nameof(IGateTargets.DetectAffectedProducts)),
                new(nameof(IGateTargets.PackAppA))
                {
                    Options = [AffectedIf("appa")],
                },
                new(nameof(IGateTargets.PackAppB))
                {
                    Options = [AffectedIf("appb")],
                },
                new(nameof(IGateTargets.ValidateGate))
                {
                    Options = [BuildOptions.Target.RunIfAnyDependencyFailed],
                },
            ],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },

        // A job that runs whatever the result of its dependencies
        new("run-always-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets =
            [
                new(nameof(IGateTargets.DetectAffectedProducts)),
                new(nameof(IGateTargets.PackAppA)),
                new(nameof(IGateTargets.ReportResults))
                {
                    Options = [BuildOptions.Target.RunAlways],
                },
            ],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];

    private StringExpr AffectedProductsOutput =>
        Tex.ParamOutput(this, nameof(IGateTargets.DetectAffectedProducts), nameof(IGateTargets.AffectedProducts));

    // A custom condition replaces the default succeeded() on Azure DevOps, so it is added back
    private TargetCondition AffectedIf(string product) =>
        BuildOptions.Target.RunIfWorkflowCondition(Tex.Success() && AffectedProductsOutput.Contains($",{product},"));
}

public interface IGateTargets : IBuildAccessor
{
    [ParamDefinition("affected-products", "Comma-wrapped list of affected products, e.g. ',appa,shared,'")]
    string? AffectedProducts => GetParam(() => AffectedProducts);

    Target DetectAffectedProducts =>
        t => t
            .ProducesVariable(nameof(AffectedProducts));

    Target PackAppA =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts));

    Target PackAppB =>
        t => t
            .DependsOn(nameof(DetectAffectedProducts));

    Target ValidateGate =>
        t => t
            .DependsOn(nameof(PackAppA))
            .DependsOn(nameof(PackAppB))
            .Executes(() => throw new InvalidOperationException("A product job failed or was cancelled."));

    Target ReportResults =>
        t => t
            .DependsOn(nameof(PackAppA));
}

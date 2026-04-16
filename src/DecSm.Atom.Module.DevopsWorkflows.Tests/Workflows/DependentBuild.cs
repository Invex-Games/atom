namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DependentBuild : MinimalBuildDefinition, IDevopsWorkflows, IDependentTarget1, IDependentTarget2
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("dependent-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets = [WorkflowTargets.DependentTarget1, WorkflowTargets.DependentTarget2],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

public interface IDependentTarget1
{
    Target DependentTarget1 => t => t;
}

public interface IDependentTarget2
{
    Target DependentTarget2 => t => t.DependsOn(nameof(IDependentTarget1.DependentTarget1));
}

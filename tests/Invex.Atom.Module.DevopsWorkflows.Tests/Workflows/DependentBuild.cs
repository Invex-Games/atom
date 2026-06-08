namespace Invex.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class DependentBuild : WorkflowBuildDefinition,
    IWorkflowBuildDefinition,
    IDevopsWorkflows,
    IDependentTarget1,
    IDependentTarget2
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
            Targets =
            [
                new(nameof(IDependentTarget1.DependentTarget1)), new(nameof(IDependentTarget2.DependentTarget2)),
            ],
            Types = [WorkflowTypes.Devops.Pipeline],
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

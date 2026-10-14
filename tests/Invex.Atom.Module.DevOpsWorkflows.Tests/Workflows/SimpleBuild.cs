namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : WorkflowBuildDefinition, IDevOpsWorkflows, ISimpleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("simple-workflow")
        {
            Triggers =
            [
                new GitPullRequestTrigger
                {
                    IncludedBranches = ["main"],
                },
            ],
            Targets = [new(nameof(ISimpleTarget.SimpleTarget))],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];
}

public interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}

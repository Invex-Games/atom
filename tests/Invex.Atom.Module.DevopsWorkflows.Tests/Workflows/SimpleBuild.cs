namespace Invex.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : WorkflowBuildDefinition, IDevopsWorkflows, ISimpleTarget
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
            Types = [WorkflowTypes.Devops.Pipeline],
        },
    ];
}

public interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}

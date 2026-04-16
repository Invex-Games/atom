namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : MinimalBuildDefinition, IGithubWorkflows, ISimpleTarget
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
            Targets = [WorkflowTargets.SimpleTarget],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

public interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}

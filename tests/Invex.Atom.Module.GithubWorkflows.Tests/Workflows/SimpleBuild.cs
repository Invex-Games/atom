namespace Invex.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : WorkflowBuildDefinition, IGithubWorkflows, ISimpleTarget
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
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}

public interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}

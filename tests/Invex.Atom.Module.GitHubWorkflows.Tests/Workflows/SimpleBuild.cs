namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SimpleBuild : WorkflowBuildDefinition, IGitHubWorkflows, ISimpleTarget
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
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface ISimpleTarget
{
    Target SimpleTarget => t => t;
}

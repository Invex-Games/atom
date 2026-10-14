namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class PullRequestAutoCancelBuild : WorkflowBuildDefinition, IDevOpsWorkflows, IPullRequestTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("pull-request-auto-cancel")
        {
            Triggers = [WorkflowTriggers.PullInto("main", "develop")],
            Targets = [new(nameof(IPullRequestTarget.PullRequestTarget))],
            Types = [WorkflowTypes.DevOps.Pipeline],
            Options = [BuildOptions.DevOps.PullRequest.AutoCancel],
        },
    ];
}

public interface IPullRequestTarget
{
    Target PullRequestTarget => t => t;
}

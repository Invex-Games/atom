namespace Invex.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class PullRequestAutoCancelBuild : WorkflowBuildDefinition, IDevopsWorkflows, IPullRequestTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("pull-request-auto-cancel")
        {
            Triggers = [WorkflowTriggers.PullInto("main", "develop")],
            Targets = [new(nameof(IPullRequestTarget.PullRequestTarget))],
            Types = [WorkflowTypes.Devops.Pipeline],
            Options = [BuildOptions.Devops.PullRequest.AutoCancel],
        },
    ];
}

public interface IPullRequestTarget
{
    Target PullRequestTarget => t => t;
}

namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ReleaseTriggerBuild : WorkflowBuildDefinition, IGitHubWorkflows, IReleaseTriggerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("releasetrigger-workflow")
        {
            Triggers = [WorkflowTriggers.GitHub.OnRelease()],
            Targets = [new(nameof(IReleaseTriggerTarget.ReleaseTriggerTarget))],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface IReleaseTriggerTarget
{
    Target ReleaseTriggerTarget => t => t;
}

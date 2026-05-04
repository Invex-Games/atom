namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ReleaseTriggerBuild : WorkflowBuildDefinition, IGithubWorkflows, IReleaseTriggerTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("releasetrigger-workflow")
        {
            Triggers = [WorkflowTriggers.Github.OnRelease()],
            Targets = [new(nameof(IReleaseTriggerTarget.ReleaseTriggerTarget))],
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}

public interface IReleaseTriggerTarget
{
    Target ReleaseTriggerTarget => t => t;
}

namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class SingleTargetBuild : WorkflowBuildDefinition, ISingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("single-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [new(nameof(ISingleTarget.SingleTarget))],
            Types = [new TestWorkflowType()],
        },
    ];
}

public interface ISingleTarget
{
    Target SingleTarget => t => t.Executes(() => Task.CompletedTask);
}

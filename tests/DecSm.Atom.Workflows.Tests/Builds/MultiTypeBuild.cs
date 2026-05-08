namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class MultiTypeBuild : WorkflowBuildDefinition, IWorkflowBuildDefinition, ISingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("multi-type-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [new(nameof(ISingleTarget.SingleTarget))],
            Types = [new TestWorkflowType(), new TestWorkflowType()],
        },
    ];
}

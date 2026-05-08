namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class EmptyTargetsWorkflowBuild : WorkflowBuildDefinition, IWorkflowBuildDefinition
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("empty-targets-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [],
            Types = [new TestWorkflowType()],
        },
    ];
}

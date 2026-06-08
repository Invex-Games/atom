namespace Invex.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class WorkflowWithOptionsBuild : WorkflowBuildDefinition, IWorkflowBuildDefinition, ISingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("options-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Options = [new TestWorkflowOption("workflow-level")],
            Targets =
            [
                new WorkflowTargetDefinition(nameof(ISingleTarget.SingleTarget)).WithOptions(
                    new TestWorkflowOption("target-level")),
            ],
            Types = [new TestWorkflowType()],
        },
    ];
}

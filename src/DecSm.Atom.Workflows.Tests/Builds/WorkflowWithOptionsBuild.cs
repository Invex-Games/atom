using DecSm.Atom.Workflows.Tests.TestUtils;

namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class WorkflowWithOptionsBuild : WorkflowBuildDefinition, ISingleTarget
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

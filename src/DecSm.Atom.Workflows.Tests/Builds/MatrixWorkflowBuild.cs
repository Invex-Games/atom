namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class MatrixWorkflowBuild : WorkflowBuildDefinition, ISingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("matrix-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets =
            [
                new WorkflowTargetDefinition(nameof(ISingleTarget.SingleTarget)).WithMatrixDimensions(
                    new MatrixDimension("os")
                    {
                        Values = ["ubuntu-latest", "windows-latest"],
                    }),
            ],
            Types = [new TestWorkflowType()],
        },
    ];
}

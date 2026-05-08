namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public partial class EmptyWorkflowBuild : WorkflowBuildDefinition
{
    // No workflows
    public override IReadOnlyList<WorkflowDefinition> Workflows => [];
}

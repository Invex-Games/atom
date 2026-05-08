namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class MinimalBuild : WorkflowBuildDefinition
{
    public override IReadOnlyList<WorkflowDefinition> Workflows => [];
}

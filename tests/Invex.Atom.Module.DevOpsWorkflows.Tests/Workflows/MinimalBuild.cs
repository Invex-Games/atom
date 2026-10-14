namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class MinimalBuild : WorkflowBuildDefinition
{
    public override IReadOnlyList<WorkflowDefinition> Workflows => [];
}

namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EmptyBuild : WorkflowBuildDefinition
{
    public override IReadOnlyList<WorkflowDefinition> Workflows => [];
}

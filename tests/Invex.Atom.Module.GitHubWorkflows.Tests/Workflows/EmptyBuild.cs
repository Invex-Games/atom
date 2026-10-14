namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EmptyBuild : WorkflowBuildDefinition
{
    public override IReadOnlyList<WorkflowDefinition> Workflows => [];
}

namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : MinimalBuildDefinition, IGithubWorkflows, IEnvironmentTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("environment-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets =
            [
                WorkflowTargets.EnvironmentTarget.WithOptions(WorkflowOptions.Deploy.ToEnvironment("test-env-1")),
            ],
            WorkflowTypes = [new GithubWorkflowType()],
        },
    ];
}

public interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}

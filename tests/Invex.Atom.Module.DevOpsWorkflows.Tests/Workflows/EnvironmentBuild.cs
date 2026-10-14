namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : WorkflowBuildDefinition, IDevOpsWorkflows, IEnvironmentTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("environment-workflow")
        {
            Triggers = [WorkflowTriggers.Manual],
            Targets =
            [
                new(nameof(IEnvironmentTarget.EnvironmentTarget))
                {
                    Options = [BuildOptions.Deploy.ToEnvironment("test-env-1")],
                },
            ],
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];
}

public interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}

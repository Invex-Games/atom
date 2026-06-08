namespace Invex.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : WorkflowBuildDefinition, IDevopsWorkflows, IEnvironmentTarget
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
            Types = [WorkflowTypes.Devops.Pipeline],
        },
    ];
}

public interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}

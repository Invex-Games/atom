namespace Invex.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : WorkflowBuildDefinition, IGithubWorkflows, IEnvironmentTarget
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
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}

public interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}

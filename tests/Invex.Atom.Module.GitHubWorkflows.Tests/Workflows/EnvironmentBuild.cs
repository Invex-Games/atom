namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class EnvironmentBuild : WorkflowBuildDefinition, IGitHubWorkflows, IEnvironmentTarget
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
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface IEnvironmentTarget
{
    Target EnvironmentTarget => t => t;
}

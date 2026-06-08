namespace Invex.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : WorkflowBuildDefinition, IGithubWorkflows, ISetupDotnetTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("setup-dotnet")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets =
            [
                new(nameof(ISetupDotnetTarget.SetupDotnetTarget))
                {
                    Options = [BuildOptions.Steps.SetupDotnet.Dotnet90X()],
                },
            ],
            Types = [WorkflowTypes.Github.Action],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

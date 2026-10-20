namespace Invex.Atom.Module.GitHubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : WorkflowBuildDefinition, IGitHubWorkflows, ISetupDotnetTarget
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
            Types = [WorkflowTypes.GitHub.Action],
        },
        new("setup-dotnet-variable")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets =
            [
                new(nameof(ISetupDotnetTarget.SetupDotnetTarget))
                {
                    Options = [BuildOptions.Steps.SetupDotnet.From(Tex.Variable("DOTNET_VERSION"))],
                },
            ],
            Types = [WorkflowTypes.GitHub.Action],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

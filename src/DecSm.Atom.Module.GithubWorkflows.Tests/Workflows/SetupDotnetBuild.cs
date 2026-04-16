namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : MinimalBuildDefinition, IGithubWorkflows, ISetupDotnetTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("setup-dotnet")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [WorkflowTargets.SetupDotnetTarget.WithOptions(WorkflowOptions.Steps.SetupDotnet.Dotnet90X())],
            WorkflowTypes = [WorkflowTypes.Github.Action],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

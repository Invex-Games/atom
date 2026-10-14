namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : WorkflowBuildDefinition, IDevOpsWorkflows, ISetupDotnetTarget
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
            Types = [WorkflowTypes.DevOps.Pipeline],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

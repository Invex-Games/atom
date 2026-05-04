namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : WorkflowBuildDefinition, IDevopsWorkflows, ISetupDotnetTarget
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
            Types = [WorkflowTypes.Devops.Pipeline],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

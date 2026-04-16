namespace DecSm.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : MinimalBuildDefinition, IDevopsWorkflows, ISetupDotnetTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("setup-dotnet")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [WorkflowTargets.SetupDotnetTarget.WithOptions(WorkflowOptions.Steps.SetupDotnet.Dotnet90X())],
            WorkflowTypes = [Devops.WorkflowType],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

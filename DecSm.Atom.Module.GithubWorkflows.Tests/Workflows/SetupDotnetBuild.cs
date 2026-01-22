namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class SetupDotnetBuild : MinimalBuildDefinition, IGithubWorkflows, ISetupDotnetTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("setup-dotnet")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [WorkflowTargets.SetupDotnetTarget.WithOptions(new SetupDotnetStep("9.0.x"))],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}

public interface ISetupDotnetTarget
{
    Target SetupDotnetTarget => t => t;
}

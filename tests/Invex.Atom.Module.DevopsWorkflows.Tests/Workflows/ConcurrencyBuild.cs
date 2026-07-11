namespace Invex.Atom.Module.DevopsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ConcurrencyBuild : WorkflowBuildDefinition, IDevopsWorkflows, IConcurrencyTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("concurrency-workflow")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(IConcurrencyTarget.ConcurrencyTarget))],
            Types = [WorkflowTypes.Devops.Pipeline],
            Options = [BuildOptions.Devops.Concurrency.Sequential],
        },
    ];
}

public interface IConcurrencyTarget
{
    Target ConcurrencyTarget => t => t;
}

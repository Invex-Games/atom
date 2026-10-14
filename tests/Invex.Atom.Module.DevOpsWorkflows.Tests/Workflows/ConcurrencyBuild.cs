namespace Invex.Atom.Module.DevOpsWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ConcurrencyBuild : WorkflowBuildDefinition, IDevOpsWorkflows, IConcurrencyTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("concurrency-workflow")
        {
            Triggers = [WorkflowTriggers.PushToMain],
            Targets = [new(nameof(IConcurrencyTarget.ConcurrencyTarget))],
            Types = [WorkflowTypes.DevOps.Pipeline],
            Options = [BuildOptions.DevOps.Concurrency.Sequential],
        },
    ];
}

public interface IConcurrencyTarget
{
    Target ConcurrencyTarget => t => t;
}

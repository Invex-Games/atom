namespace DecSm.Atom.Workflows.Tests.Builds;

[BuildDefinition]
public interface IInterfaceBuild : IWorkflowBuildDefinition, IInterfaceTarget
{
    IReadOnlyList<WorkflowDefinition> IWorkflowBuildDefinition.Workflows =>
    [
        new("single-workflow")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [new(nameof(InterfaceTarget))],
            Types = [new TestWorkflowType()],
        },
    ];
}

public interface IInterfaceTarget
{
    Target InterfaceTarget => t => t.Executes(() => Task.CompletedTask);
}

namespace DecSm.Atom.Tests.BuildTests.Workflows;

[BuildDefinition]
public partial class WorkflowDependentTargetBuild : MinimalBuildDefinition,
    IWorkflowDependentTarget1,
    IWorkflowDependentTarget2
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("workflow-2")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [WorkflowTargets.WorkflowTarget2],
            Options = [new TestWorkflowOption()],
            WorkflowTypes = [new TestWorkflowType()],
        },
    ];
}

public interface IWorkflowDependentTarget1
{
    Target WorkflowDependentTarget1 =>
        t => t
            .DescribedAs("Workflow Target 1")
            .Executes(() => Task.CompletedTask);
}

public interface IWorkflowDependentTarget2
{
    Target WorkflowTarget2 =>
        t => t
            .DescribedAs("Workflow Target 2")
            .DependsOn(nameof(IWorkflowDependentTarget1.WorkflowDependentTarget1))
            .Executes(() => Task.CompletedTask);
}

namespace DecSm.Atom.Tests.BuildTests.Workflows;

[BuildDefinition]
public partial class WorkflowSingleTargetBuild : MinimalBuildDefinition, IWorkflowSingleTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("workflow-1")
        {
            Triggers = [new TestWorkflowTrigger()],
            Targets = [WorkflowTargets.WorkflowSingleTarget],
            Options = [new TestWorkflowOption()],
            WorkflowTypes = [new TestWorkflowType()],
        },
    ];
}

public interface IWorkflowSingleTarget
{
    Target WorkflowSingleTarget =>
        t => t
            .DescribedAs("Workflow Target 1")
            .Executes(() => Task.CompletedTask);
}
